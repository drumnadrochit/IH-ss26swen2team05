import {
  Component, Input, Output, EventEmitter,
  signal, OnInit, ChangeDetectionStrategy
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TourLog, Difficulty } from '../../models/tour_log';
import { CreateTourLogRequest, UpdateTourLogRequest } from '../../models/API/tour_log_api';

@Component({
  selector: 'app-tour-log-form',
  standalone: true,
  imports: [FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './tour-log-form.html',
  styleUrls: ['./tour-log-form.scss']
})
export class TourLogFormComponent implements OnInit {

  @Input() log: TourLog | null = null;
  @Input() tourId: string = '';

  @Output() logSaved = new EventEmitter<CreateTourLogRequest | UpdateTourLogRequest>();
  @Output() cancel = new EventEmitter<void>();

  isEditMode = signal(false);

  dateTime = signal('');
  comment = signal('');
  difficulty = signal<Difficulty>(Difficulty.Medium);
  totalDistance = signal<number>(0);
  totalTime = signal<number>(0);
  rating = signal<number>(3);

  errors = signal<Record<string, string>>({});

  difficulties = [
    { value: Difficulty.VeryEasy, label: 'Very Easy' },
    { value: Difficulty.Easy, label: 'Easy' },
    { value: Difficulty.Medium, label: 'Medium' },
    { value: Difficulty.Hard, label: 'Hard' },
    { value: Difficulty.Extreme, label: 'Extreme' }
  ];

  ngOnInit(): void {
    if (this.log) {
      this.isEditMode.set(true);
      this.dateTime.set(this.log.dateTime.slice(0, 16)); // format for datetime-local
      this.comment.set(this.log.comment);
      this.difficulty.set(this.log.difficulty);
      this.totalDistance.set(this.log.totalDistance);
      this.totalTime.set(this.log.totalTime);
      this.rating.set(this.log.rating);
    } else {
      // Default to now
      const now = new Date();
      this.dateTime.set(now.toISOString().slice(0, 16));
    }
  }

  onFieldChange(field: string, event: Event): void {
    const value = (event.target as HTMLInputElement | HTMLTextAreaElement).value;
    switch (field) {
      case 'dateTime': this.dateTime.set(value); break;
      case 'comment': this.comment.set(value); break;
      case 'totalDistance': this.totalDistance.set(parseFloat(value) || 0); break;
      case 'totalTime': this.totalTime.set(parseInt(value) || 0); break;
    }
    this.errors.update(e => { const c = { ...e }; delete c[field]; return c; });
  }

  setDifficulty(diff: Difficulty): void {
    this.difficulty.set(diff);
  }

  setRating(stars: number): void {
    this.rating.set(stars);
  }

  validate(): boolean {
    const errs: Record<string, string> = {};
    if (!this.dateTime()) errs['dateTime'] = 'Date is required';
    if (this.totalDistance() <= 0) errs['totalDistance'] = 'Distance must be greater than 0';
    if (this.totalTime() <= 0) errs['totalTime'] = 'Time must be greater than 0';
    this.errors.set(errs);
    return Object.keys(errs).length === 0;
  }

  onSave(): void {
    if (!this.validate()) return;

    const base = {
      accomplishedAt: new Date(this.dateTime()).toISOString(),
      comment: this.comment().trim(),
      difficulty: this.difficulty(),
      totalDistanceKm: this.totalDistance(),
      totalTimeMinutes: this.totalTime(),
      rating: this.rating(),
    };

    const payload: CreateTourLogRequest | UpdateTourLogRequest = this.log
      ? { tourLogId: this.log.id, ...base }
      : { tourId: this.tourId, ...base };

    this.logSaved.emit(payload);
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
