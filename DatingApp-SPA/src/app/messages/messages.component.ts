import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
