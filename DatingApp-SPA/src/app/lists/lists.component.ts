import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
