import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css'],
    imports: [FormsModule]
})
export class RegisterComponent implements OnInit {
  // to receive prop from parent comp
  // to send prop for parent comp
  @Output() cancelRegister = new EventEmitter();
  model: any = {};

  constructor(private authervice: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  register() {
    this.authervice.register(this.model).subscribe(() => {
      this.alertify.success('registerd successfully');
    }, error => {
      this.alertify.error(error);
    });
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
