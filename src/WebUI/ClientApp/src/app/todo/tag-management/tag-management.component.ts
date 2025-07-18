import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { TagsClient, TagDto, CreateTagCommand } from '../../web-api-client';

@Component({
  selector: 'app-tag-management',
  templateUrl: './tag-management.component.html',
  styleUrls: ['./tag-management.component.scss']
})
export class TagManagementComponent implements OnInit {
  @Output() tagCreated = new EventEmitter<TagDto>();
  @Output() tagDeleted = new EventEmitter<number>();

  tags: TagDto[] = [];
  showCreateForm = false;
  newTag: CreateTagCommand = new CreateTagCommand({
    name: '',
    color: '#6b7280'
  });

  predefinedColors = [
    { name: 'Gray', value: '#6b7280' },
    { name: 'Red', value: '#ef4444' },
    { name: 'Orange', value: '#f97316' },
    { name: 'Yellow', value: '#eab308' },
    { name: 'Green', value: '#22c55e' },
    { name: 'Blue', value: '#3b82f6' },
    { name: 'Purple', value: '#a855f7' },
    { name: 'Pink', value: '#ec4899' }
  ];

  constructor(private tagsClient: TagsClient) { }

  ngOnInit(): void {
    this.loadTags();
  }

  loadTags(): void {
    this.tagsClient.getTags().subscribe({
      next: (tags) => {
        this.tags = tags;
      },
      error: (error) => {
        console.error('Error loading tags:', error);
      }
    });
  }

  createTag(): void {
    if (!this.newTag.name?.trim()) {
      return;
    }

    this.tagsClient.create(this.newTag).subscribe({
      next: (tagId) => {
        const createdTag = new TagDto({
          id: tagId,
          name: this.newTag.name,
          color: this.newTag.color,
          created: new Date()
        });

        this.tags.push(createdTag);
        this.tagCreated.emit(createdTag);
        this.resetCreateForm();
      },
      error: (error) => {
        console.error('Error creating tag:', error);
      }
    });
  }

  deleteTag(tag: TagDto): void {
    if (confirm(`Are you sure you want to delete the tag "${tag.name}"?`)) {
      this.tagsClient.delete(tag.id!).subscribe({
        next: () => {
          this.tags = this.tags.filter(t => t.id !== tag.id);
          this.tagDeleted.emit(tag.id);
        },
        error: (error) => {
          console.error('Error deleting tag:', error);
        }
      });
    }
  }

  resetCreateForm(): void {
    this.newTag = new CreateTagCommand({
      name: '',
      color: '#6b7280'
    });
    this.showCreateForm = false;
  }

  selectColor(color: string): void {
    this.newTag.color = color;
  }
}
