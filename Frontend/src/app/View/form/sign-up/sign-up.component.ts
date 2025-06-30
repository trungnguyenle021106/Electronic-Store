import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../../../Service/Auth/auth.service';

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
      Gender: ['', Validators.required],
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
      const formData = this.registerForm.value;
      console.log('Submitting form:', formData);

      // Replace with actual API call
      this.authService.signUp(formData).subscribe({
        next: (response) => {
          this.authService.setLoggedIn(true);
        },
        error: (error) => {
          this.backendError = error.error.message || 'Đăng nhập thất bại. Vui lòng thử lại.';
        },
      })
    } else {
      // Mark all fields as touched to show errors
      this.registerForm.markAllAsTouched();
    }
  }

}
