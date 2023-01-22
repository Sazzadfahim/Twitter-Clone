using System.Threading.Channels;

namespace TweetRetweetCommentLike.Services
{
    public class RabbitMQDeleteServices : IRabbitMqDeleteService,IDisposable
    {
        private readonly IFollowBlockIndividualServices _followBlockIndividual;
        protected IModel _channel;
        private readonly IConnection _connection;
        private IMongoCollection<CommentLikeRetweet> _likeCommentRetweet;
        private IMongoCollection<TweetSearchDto> _tweetSearch;
        private readonly ILikeCommentRetweetServices _likeCommentRetweetService;
        public RabbitMQDeleteServices(IFollowBlockIndividualServices followBlockIndividual,
            IOptions<DatabaseSetting.DatabaseSetting> db,
            ILikeCommentRetweetServices likeCommentRetweetService, IRabbitMQService mqService)
        {
            _connection=mqService.CreateChannel();
            var client = new MongoClient(db.Value.connectionString);
            var database = client.GetDatabase(db.Value.databaseName);
            _followBlockIndividual = followBlockIndividual;
            _likeCommentRetweetService = likeCommentRetweetService;
            _tweetSearch = database.GetCollection<TweetSearchDto>(db.Value.hashSearchCollection);
        }

        public async Task Send(string userName, string tweetId)
        {
            
            var followers = new List<string>();
            if (await _followBlockIndividual.GetAllFollowers(userName) == null)
            {
                followers.Add(userName);
            }
            else
            {
                followers = await _followBlockIndividual.GetAllFollowers(userName);
                followers.Add(userName);
            }
            await _likeCommentRetweetService.Delete(tweetId);
            await _tweetSearch.FindOneAndDeleteAsync(x => x.tweetId == tweetId);
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare("Dopaminedelete:" + userName, ExchangeType.Fanout);
            foreach (var follower in followers)
            {
                _channel.QueueDeclare("Dopaminedelete:" + follower, true, false, false);
                _channel.QueueBind("Dopaminedelete:" + follower, "Dopaminedelete:" + userName, follower);
            }

            byte[] body = Encoding.UTF8.GetBytes(tweetId);
            _channel.BasicPublish(exchange: "Dopaminedelete:" + userName,
                routingKey: "",
                basicProperties: null,
                body: body);
            foreach (var follower in followers)
            {
                _channel.QueueUnbind(follower, "Dopaminedelete:" + userName, follower);
            }

            _channel.ExchangeDelete("Dopaminedelete:" + userName);
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
                _channel.Close();
            if (_connection.IsOpen)
                _connection.Close();
        }
    }
}
