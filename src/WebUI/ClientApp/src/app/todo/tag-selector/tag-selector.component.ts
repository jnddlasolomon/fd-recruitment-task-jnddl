import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { TagsClient, TodoItemsClient, TagDto, TodoItemBriefDto } from '../../web-api-client';

@Component({
  selector: 'app-tag-selector',
  templateUrl: './tag-selector.component.html',
  styleUrls: ['./tag-selector.component.scss']
})
export class TagSelectorComponent implements OnInit {
  @Input() todoItem!: TodoItemBriefDto; // Changed from TodoItemDto to TodoItemBriefDto
  @Input() currentTags: TagDto[] = [];
  @Output() tagsUpdated = new EventEmitter<TagDto[]>();

  availableTags: TagDto[] = [];
  showTagDropdown = false;

  constructor(
    private tagsClient: TagsClient,
    private todoItemsClient: TodoItemsClient
  ) { }

  ngOnInit(): void {
    this.loadAvailableTags();
    // Use the passed-in current tags instead of loading them
    this.currentTags = this.currentTags || [];
  }

  loadAvailableTags(): void {
    this.tagsClient.getTags().subscribe({
      next: (tags) => {
        this.availableTags = tags;
      },
      error: (error) => {
        console.error('Error loading tags:', error);
      }
    });
  }

  loadCurrentTags(): void {
    // Since TodoItemBriefDto doesn't have tags, we'll need to fetch them
    // For now, start with empty array - in a real app you'd fetch from the API
    this.currentTags = [];
  }

  isTagSelected(tag: TagDto): boolean {
    return this.currentTags.some(t => t.id === tag.id);
  }

  toggleTag(tag: TagDto): void {
    if (this.isTagSelected(tag)) {
      this.removeTag(tag);
    } else {
      this.addTag(tag);
    }
  }

  addTag(tag: TagDto): void {
    this.todoItemsClient.addTag(this.todoItem.id!, tag.id!).subscribe({
      next: () => {
        this.currentTags.push(tag);
        this.tagsUpdated.emit(this.currentTags);
      },
      error: (error) => {
        console.error('Error adding tag:', error);
      }
    });
  }

  removeTag(tag: TagDto): void {
    this.todoItemsClient.removeTag(this.todoItem.id!, tag.id!).subscribe({
      next: () => {
        this.currentTags = this.currentTags.filter(t => t.id !== tag.id);
        this.tagsUpdated.emit(this.currentTags);
      },
      error: (error) => {
        console.error('Error removing tag:', error);
      }
    });
  }

  toggleDropdown(): void {
    this.showTagDropdown = !this.showTagDropdown;
  }

  closeDropdown(): void {
    this.showTagDropdown = false;
  }
}
