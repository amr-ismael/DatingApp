import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { RegisterComponent } from '../register/register.component';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
    styleUrls: ['./home.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager,
    imports: [RegisterComponent]
})
export class HomeComponent implements OnInit {
  registerMode = false;

  ngOnInit() {
  }

  registerToggle() {
    this.registerMode = true;
  }

  cancelRegisterMode(registerMode: boolean) {
    this.registerMode = registerMode;
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !!token;
  }


}

