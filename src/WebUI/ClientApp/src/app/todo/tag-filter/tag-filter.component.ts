import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { TagsClient, TagDto } from '../../web-api-client';

export interface FilterState {
  selectedTagIds: number[];
  searchTerm: string;
}

@Component({
  selector: 'app-tag-filter',
  templateUrl: './tag-filter.component.html',
  styleUrls: ['./tag-filter.component.scss']
})
export class TagFilterComponent implements OnInit {
  @Input() currentFilter: FilterState = { selectedTagIds: [], searchTerm: '' };
  @Output() filterChanged = new EventEmitter<FilterState>();

  availableTags: TagDto[] = [];
  searchTerm = '';

  constructor(private tagsClient: TagsClient) { }

  ngOnInit(): void {
    this.loadTags();
    this.searchTerm = this.currentFilter.searchTerm;
  }

  loadTags(): void {
    this.tagsClient.getTags().subscribe({
      next: (tags) => {
        this.availableTags = tags;
      },
      error: (error) => {
        console.error('Error loading tags:', error);
      }
    });
  }

  isTagSelected(tagId: number): boolean {
    return this.currentFilter.selectedTagIds.includes(tagId);
  }

  toggleTag(tag: TagDto): void {
    const tagId = tag.id!;
    const newSelectedTagIds = this.isTagSelected(tagId)
      ? this.currentFilter.selectedTagIds.filter(id => id !== tagId)
      : [...this.currentFilter.selectedTagIds, tagId];

    this.emitFilterChange(newSelectedTagIds, this.searchTerm);
  }

  onSearchChange(): void {
    this.emitFilterChange(this.currentFilter.selectedTagIds, this.searchTerm);
  }

  clearAllFilters(): void {
    this.searchTerm = '';
    this.emitFilterChange([], '');
  }

  clearTagFilters(): void {
    this.emitFilterChange([], this.searchTerm);
  }

  clearSearchFilter(): void {
    this.searchTerm = '';
    this.emitFilterChange(this.currentFilter.selectedTagIds, '');
  }

  private emitFilterChange(selectedTagIds: number[], searchTerm: string): void {
    const newFilter: FilterState = {
      selectedTagIds,
      searchTerm: searchTerm.trim()
    };

    this.currentFilter = newFilter;
    this.filterChanged.emit(newFilter);
  }

  getSelectedTags(): TagDto[] {
    return this.availableTags.filter(tag =>
      this.currentFilter.selectedTagIds.includes(tag.id!)
    );
  }

  hasActiveFilters(): boolean {
    return this.currentFilter.selectedTagIds.length > 0 ||
      this.currentFilter.searchTerm.length > 0;
  }
}
