import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../Service/Auth/auth.service';
import { SignUpRequest } from '../../../Model/Requests/SignUpRequest';

@Component({
  selector: 'app-sign-up',
  standalone: false,
  templateUrl: './sign-up.component.html',
  styleUrl: './sign-up.component.css'
})
export class SignUpComponent {
  @Output() statusSignUpEmitter = new EventEmitter<boolean>();

  registerForm: FormGroup;
  backendError: string | null = null;

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.registerForm = this.fb.group({
      Email: ['', [Validators.required, Validators.email]],
      Name: ['', Validators.required],
      Phone: [
        '',
        [
          Validators.required,
          Validators.pattern('^[0-9]*$'),
          Validators.minLength(10),
          Validators.maxLength(10)
        ]],
      Address: ['', Validators.required],
      Password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(50)]],
    });
  }

  CloseSignUpForm(): void {
    this.statusSignUpEmitter.emit(false);
  }

  // Check if a form field is invalid and touched
  isFieldInvalid(field: string): boolean {
    const control = this.registerForm.get(field);
    return (control?.invalid && (control.touched || control.dirty)) ?? false;
  }

  // Submit form
  onSubmit(): void {
    if (this.registerForm.valid) {
      // Reset backend error
      this.backendError = null;

      // Simulate API call
      const signUpRequest: SignUpRequest = this.registerForm.value;
      console.log('Submitting form:', signUpRequest);

      // Replace with actual API call
      this.authService.signUp(signUpRequest).subscribe({
        next: (response) => {
          this.authService.setLoggedIn(true);
        },
        error: (error) => {
          console.log(error)
          this.backendError = error.error.message || 'Đăng nhập thất bại. Vui lòng thử lại.';
        },
      })
    } else {
      // Mark all fields as touched to show errors
      this.registerForm.markAllAsTouched();
    }
  }

}
