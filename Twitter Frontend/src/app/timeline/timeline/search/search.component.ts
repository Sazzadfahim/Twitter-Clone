import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SearchService } from 'src/app/Services/search.service';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent implements OnInit {

  users:any[] = [];
  tweets:any[] = [];
  keyword = 'userName';
  timeout:any=null;
  searchInput: String = '';
  searchResult: Array<any> = [];
  toggle: Boolean = false;
  selectedInput: any = {};
  userS=false;
  tweetS=false;

  constructor(private searchService : SearchService, private route : Router) { }

  ngOnInit(): void {
    
  }


  onKeySearch(event: any) {
    clearTimeout(this.timeout);
    var $this = this;
    this.timeout = setTimeout(function () {
      if (event.keyCode != 13) {
        $this.executeListing(event.target.value);
      }
    }, 1000);
  }

  private executeListing(value: string) {
    if(value=='')
    {
      this.users=[];
      this.tweets=[];
    }
   if(value.startsWith("#"))
   {
    this.tweetS=true;
    this.searchService.getTweetByHashTag(value).subscribe((val:any)=>
    {
      this.users=[];
      this.tweets=val;
    },
    error=>
    {
      alert("Server Error")
    })
   }
   else
   {
    this.userS=true;
    this.searchService.getUsers(value).subscribe((val:any)=>
      {
        this.tweets=[];
        this.users=val;
      },
      err=>
      {
        
      }

    );
   }
  }

  searchUser(userName : any)
  {
    this.route.navigate(['timeline/profile/'+userName.toString()])
  }

  searchTweet(userName:any,tweetId:any)
  {
    this.route.navigate(['timeline/tweet/'+userName.toString()+'/'+tweetId.toString()])
  }

}
