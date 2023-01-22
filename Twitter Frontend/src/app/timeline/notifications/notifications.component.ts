import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Observable,interval } from 'rxjs';
import { NotificationsService } from 'src/app/Services/notifications.service';

@Component({
  selector: 'app-notifications',
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.css']
})
export class NotificationsComponent implements OnInit ,OnDestroy{

  notifications$ :any;
  thisUser:any;
  currentUser='';
  timer:any;
  page = 1;
  constructor(
    private route : Router,
    private notificationServices : NotificationsService,
    
  ) { }


  ngOnInit(): void {
    this.currentUser=JSON.parse(localStorage.getItem('loginInfo')||'' )['userName'].toString() ;
    this.timer = setInterval(()=>
    {
      this.notificationServices.getAllNotifications(this.page).subscribe(
        val=>
        {
          this.notifications$=val;
        }
      )
    },10000);
    this.getInstant();
    this.notificationServices.refreshRequired.subscribe(
      response=>
      {
        this.getInstant();
      }
    );
  }

  ngOnDestroy(): void {
    if(this.timer)
    {
      clearInterval(this.timer);
    }
  }

 getInstant()
 {
  this.notificationServices.getAllNotifications(this.page).subscribe(
    val=>
    {
      this.notifications$=val;
      //alert(JSON.stringify(this.notifications$));
    }
  )
 }
 loadmore()
 { 
  this.notificationServices.getAllNotifications(this.page+1).subscribe(
    val=>
    {
      this.page++;
      this.notifications$=val;
    }
  )
 }


  gotoTweet(notification:any)
  {
    this.route.navigate(['/timeline/tweet/'+notification.receiverUserName+'/'+notification.tweetId]);
  }



}
