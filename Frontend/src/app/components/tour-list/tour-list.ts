import {
  Component, Input, Output, EventEmitter,
  signal, computed, ChangeDetectionStrategy
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Tour } from '../../models/tour';
import { PopupComponent } from '../shared/popup/popup';
import { SearchService, SearchResult } from '../../services/search';
import { formatTime } from '../../utils/format';

@Component({
  selector: 'app-tour-list',
  standalone: true,
  imports: [FormsModule, PopupComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './tour-list.html',
  styleUrls: ['./tour-list.scss']
})
export class TourListComponent {

  @Input() set tours(value: Tour[]) {
    this.allTours.set(value);
  }

  @Output() tourSelected = new EventEmitter<Tour>();
  @Output() tourCreate = new EventEmitter<void>();
  @Output() tourEdit = new EventEmitter<Tour>();
  @Output() tourDelete = new EventEmitter<Tour>();

  allTours = signal<Tour[]>([]);
  searchQuery = signal('');
  selectedTourId = signal<string | null>(null);

  showDeleteConfirm = signal(false);
  tourToDelete = signal<Tour | null>(null);

  // Real backend full-text search across tours AND logs (incl. computed values), matching
  // GET /api/search. null while no query is active or a search hasn't resolved yet.
  private searchResult = signal<SearchResult | null>(null);
  private searchDebounce?: ReturnType<typeof setTimeout>;

  constructor(private searchService: SearchService) {}

  // Empty query: just show everything, no backend round-trip needed.
  // Otherwise: a tour matches if it matched directly, or one of its logs did
  // (e.g. a log comment) — the sidebar always lists tours, never bare logs.
  filteredTours = computed(() => {
    const q = this.searchQuery().trim();
    const tours = this.allTours();
    if (!q) return tours;

    const result = this.searchResult();
    if (!result) return tours; // still debouncing/waiting on the request

    const matchedTourIds = new Set(result.tours.map(t => t.id));
    for (const log of result.tourLogs) matchedTourIds.add(log.tourId);

    return tours.filter(t => matchedTourIds.has(t.id));
  });

  deleteMessage = computed(() => {
    const tour = this.tourToDelete();
    if (!tour) return '';
    return `Are you sure you want to delete "${tour.name}"? This will also delete all associated logs.`;
  });

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);

    clearTimeout(this.searchDebounce);
    const trimmed = value.trim();
    if (!trimmed) {
      this.searchResult.set(null);
      return;
    }

    this.searchDebounce = setTimeout(() => {
      this.searchService.search(trimmed).subscribe({
        next: (result) => this.searchResult.set(result),
        error: (err) => {
          console.error('Search failed', err);
          this.searchResult.set(null);
        },
      });
    }, 250);
  }

  onSelect(tour: Tour): void {
    this.selectedTourId.set(tour.id);
    this.tourSelected.emit(tour);
  }

  onCreate(): void {
    this.tourCreate.emit();
  }

  onEdit(event: Event, tour: Tour): void {
    event.stopPropagation();
    this.tourEdit.emit(tour);
  }

  onDeleteClick(event: Event, tour: Tour): void {
    event.stopPropagation();
    this.tourToDelete.set(tour);
    this.showDeleteConfirm.set(true);
  }

  onConfirmDelete(): void {
    const tour = this.tourToDelete();
    if (tour) this.tourDelete.emit(tour);
    this.showDeleteConfirm.set(false);
    this.tourToDelete.set(null);
  }

  onCancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.tourToDelete.set(null);
  }

  formatTime = formatTime;
}
