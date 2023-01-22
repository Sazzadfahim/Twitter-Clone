namespace TweetRetweetCommentLike.Services
{
    public class RabbitMqPublish : IRabbitMqPublish
    {
        private readonly IFollowBlockIndividualServices _followBlockIndividual;
        protected IModel _channel;
        private readonly IConnection _connection;
        private IMongoCollection<CommentLikeRetweet> _likeCommentRetweet;
        private readonly IMongoCollection<TweetSearchDto> _tweetSearch;
        public RabbitMqPublish(IFollowBlockIndividualServices followBlockIndividual,IOptions<DatabaseSetting.DatabaseSetting> db,IRabbitMQService mqService)
        {
            _connection = mqService.CreateChannel();
            var client = new MongoClient(db.Value.connectionString);
            var database = client.GetDatabase(db.Value.databaseName);
            _tweetSearch = database.GetCollection<TweetSearchDto>(db.Value.hashSearchCollection);
            _likeCommentRetweet = database.GetCollection<CommentLikeRetweet>(db.Value.likeCommentRetweetCollectionName);
            _followBlockIndividual = followBlockIndividual;
        }
        public async Task Send(Tweet tweet)
        {
            
            var followers = new List<string>();
            if (await _followBlockIndividual.GetAllFollowers(tweet.userName) == null)
            {
                followers.Add(tweet.userName);
            }
            else
            {
                followers = await _followBlockIndividual.GetAllFollowers(tweet.userName);
                followers.Add(tweet.userName);
            }
            tweet.tweetId = Guid.NewGuid().ToString();

            var regex = new Regex(@"#\w+");
            var dto = new TweetSearchDto();
            
            var matches = regex.Matches(tweet.tweetText);
            if (matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    dto.key += match.ToString();
                }

                dto.tweetId = tweet.tweetId;
                dto.tweetText = tweet.tweetText;
                dto.userName = tweet.userName;

                await _tweetSearch.InsertOneAsync(dto);
            }
            
            var likeCommentRetweet = new CommentLikeRetweet()
            {
                tweetId = tweet.tweetId,
                comments = new List<CommentDto>(),
                likes = new List<string>(),
                retweets = new List<RetweetDto>()
            };
            await _likeCommentRetweet.InsertOneAsync(likeCommentRetweet);

            Dictionary<String, Object> args = new Dictionary<String, Object>();
            args.Add("x-message-ttl", 86400000);
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("Dopamine:" + tweet.userName, ExchangeType.Fanout);
            foreach (var follower in followers)
            {
                _channel.QueueDeclare("Dopamine:" + follower, true, false, false, args);
                _channel.QueueBind("Dopamine:" + follower, "Dopamine:" + tweet.userName, follower);
            }
            byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(tweet));
            _channel.BasicPublish(exchange: "Dopamine:" + tweet.userName,
                routingKey: "",
                basicProperties: null,
                body: body);
            foreach (var follower in followers)
            {
                _channel.QueueUnbind(follower, "Dopamine:" + tweet.userName, follower);
            }
            _channel.ExchangeDelete("Dopamine:" + tweet.userName);
        }
    }
}
