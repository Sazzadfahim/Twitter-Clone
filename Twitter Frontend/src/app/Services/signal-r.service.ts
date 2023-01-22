import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnectionBuilder } from '@microsoft/signalr';
import Swal from 'sweetalert2';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  token = "";
  username ="";
  constructor(private http: HttpClient) {
    this.token = JSON.parse(localStorage.getItem('loginInfo')||'' )['accessToken'];
    this.username = JSON.parse(localStorage.getItem('loginInfo')||'' )['userName'];
    this.start();
    this.connection.on("Notification",(data:any)=>
    {
      Swal.fire({
        position: 'top-end',
        title: data.message,
        showConfirmButton: false,
        timer: 5000
      })
    }); 
   }
 
   
   private  connection: any = new HubConnectionBuilder()   // mapping to the chathub as in startup.cs                                       
              .withUrl("http://dopamine.learnathon.net/api4/livenotification?username="+JSON.parse(localStorage.getItem('loginInfo')||'' )['userName'],
              {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets
              }
              )
              .withAutomaticReconnect()
              .build();

  public async start() {
    try {
      await this.connection.start();
    } catch (err) {
      setTimeout(() => this.start(), 5000);
    } 
  }
  


}
