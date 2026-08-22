import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
