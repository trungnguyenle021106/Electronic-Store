import {Component} from '@angular/core';
import {HomeComponent} from './home/home.component';
@Component({
  selector: 'app-root',
  standalone : false,
  templateUrl:'./app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  title = 'FGearVN';
}