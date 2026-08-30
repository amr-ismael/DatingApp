import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectionStrategy, inject } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager,
    imports: [FormsModule]
})
export class RegisterComponent implements OnInit {
  private authervice = inject(AuthService);
  private alertify = inject(AlertifyService);

  // to receive prop from parent comp
  // to send prop for parent comp
  @Output() cancelRegister = new EventEmitter();
  model: any = {};

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
