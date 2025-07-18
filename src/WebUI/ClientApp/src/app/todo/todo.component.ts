import { Component, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { TagDto } from '../web-api-client';
import { FilterState } from './tag-filter/tag-filter.component';

import {
  TodoListsClient, TodoItemsClient,
  TodoListDto, TodoItemBriefDto, PriorityLevelDto,
  CreateTodoListCommand, UpdateTodoListCommand,
  CreateTodoItemCommand, UpdateTodoItemDetailCommand,
  UpdateTodoItemCommand
} from '../web-api-client';

// Custom interface that extends the base properties without the class methods
interface TodoItemExtended {
  id?: number;
  listId?: number;
  title?: string;
  done?: boolean;
  backgroundColor?: string;
  priority?: number;
  note?: string;
}

@Component({
  selector: 'app-todo-component',
  templateUrl: './todo.component.html',
  styleUrls: ['./todo.component.scss']
})

export class TodoComponent implements OnInit {
  debug = false;
  deleting = false;
  deleteCountDown = 0;
  deleteCountDownInterval: any;
  lists: TodoListDto[];
  priorityLevels: PriorityLevelDto[];
  selectedList: TodoListDto;
  selectedItem: TodoItemExtended;
  selectedColorItem: any = null; // Track which item's color picker is open
  newListEditor: any = {};
  listOptionsEditor: any = {};
  newListModalRef: BsModalRef;
  listOptionsModalRef: BsModalRef;
  deleteListModalRef: BsModalRef;
  itemDetailsModalRef: BsModalRef;

  // NEW: Filtering property
  currentFilter: FilterState = { selectedTagIds: [], searchTerm: '' };

  itemDetailsFormGroup = this.fb.group({
    id: [null],
    listId: [null],
    priority: [''],
    note: ['']
  });

  private itemTags: Map<number, TagDto[]> = new Map();

  constructor(
    private listsClient: TodoListsClient,
    private itemsClient: TodoItemsClient,
    private modalService: BsModalService,
    private fb: FormBuilder
  ) { }

  ngOnInit(): void {
    this.listsClient.get().subscribe(
      result => {
        this.lists = result.lists;
        this.priorityLevels = result.priorityLevels;
        if (this.lists.length) {
          this.selectedList = this.lists[0];
        }
      },
      error => console.error(error)
    );
  }

  // Helper method to get background color safely
  getItemBackgroundColor(item: any): string {
    return (item as TodoItemExtended).backgroundColor || '#ffffff';
  }

  // Toggle color picker dropdown
  toggleColorPicker(item: any): void {
    if (this.selectedColorItem === item) {
      this.selectedColorItem = null; // Close if already open
    } else {
      this.selectedColorItem = item; // Open for this item
    }
  }

  // Lists
  remainingItems(list: TodoListDto): number {
    return list.items.filter(t => !t.done).length;
  }

  showNewListModal(template: TemplateRef<any>): void {
    this.newListModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('title')?.focus(), 250);
  }

  newListCancelled(): void {
    this.newListModalRef.hide();
    this.newListEditor = {};
  }

  addList(): void {
    const list = {
      id: 0,
      title: this.newListEditor.title,
      items: []
    } as TodoListDto;

    this.listsClient.create(list as CreateTodoListCommand).subscribe(
      result => {
        list.id = result;
        this.lists.push(list);
        this.selectedList = list;
        this.newListModalRef.hide();
        this.newListEditor = {};
      },
      error => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.newListEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('title')?.focus(), 250);
      }
    );
  }

  showListOptionsModal(template: TemplateRef<any>) {
    this.listOptionsEditor = {
      id: this.selectedList.id,
      title: this.selectedList.title
    };

    this.listOptionsModalRef = this.modalService.show(template);
  }

  updateListOptions() {
    const list = this.listOptionsEditor as UpdateTodoListCommand;
    this.listsClient.update(this.selectedList.id, list).subscribe(
      () => {
        this.selectedList.title = this.listOptionsEditor.title;
        this.listOptionsModalRef.hide();
        this.listOptionsEditor = {};
      },
      error => console.error(error)
    );
  }

  confirmDeleteList(template: TemplateRef<any>) {
    this.listOptionsModalRef.hide();
    this.deleteListModalRef = this.modalService.show(template);
  }

  deleteListConfirmed(): void {
    this.listsClient.delete(this.selectedList.id).subscribe(
      () => {
        this.deleteListModalRef.hide();
        this.lists = this.lists.filter(t => t.id !== this.selectedList.id);
        this.selectedList = this.lists.length ? this.lists[0] : null;
      },
      error => console.error(error)
    );
  }

  // Items
  showItemDetailsModal(template: TemplateRef<any>, item: any): void {
    this.selectedItem = item as TodoItemExtended;
    this.itemDetailsFormGroup.patchValue(this.selectedItem);

    this.itemDetailsModalRef = this.modalService.show(template);
    this.itemDetailsModalRef.onHidden?.subscribe(() => {
      this.stopDeleteCountDown();
    });
  }

  updateItemDetails(): void {
    const item = new UpdateTodoItemDetailCommand(this.itemDetailsFormGroup.value);
    this.itemsClient.updateItemDetails(this.selectedItem.id!, item).subscribe(
      () => {
        if (this.selectedItem.listId !== item.listId) {
          this.selectedList.items = this.selectedList.items.filter(
            i => i.id !== this.selectedItem.id
          );
          const listIndex = this.lists.findIndex(
            l => l.id === item.listId
          );
          this.selectedItem.listId = item.listId;
          // Create a proper TodoItemBriefDto for the array
          const briefItem = {
            id: this.selectedItem.id,
            listId: this.selectedItem.listId,
            title: this.selectedItem.title,
            done: this.selectedItem.done
          } as TodoItemBriefDto;
          this.lists[listIndex].items.push(briefItem);
        }

        this.selectedItem.priority = item.priority;
        this.selectedItem.note = item.note;
        this.itemDetailsModalRef.hide();
        this.itemDetailsFormGroup.reset();
      },
      error => console.error(error)
    );
  }

  addItem() {
    const item: TodoItemExtended = {
      id: 0,
      listId: this.selectedList.id,
      priority: this.priorityLevels[0].value,
      title: '',
      done: false,
      backgroundColor: '#ffffff'
    } as TodoItemExtended;

    // Convert to TodoItemBriefDto for the array
    const briefItem = new TodoItemBriefDto({
      id: item.id,
      listId: item.listId,
      title: item.title,
      done: item.done
    });

    this.selectedList.items.push(briefItem);
    const index = this.selectedList.items.length - 1;
    this.editItem(item, 'itemTitle' + index);
  }

  editItem(item: any, inputId: string): void {
    this.selectedItem = item as TodoItemExtended;
    setTimeout(() => document.getElementById(inputId)?.focus(), 100);
  }

  updateItem(item: any, pressedEnter: boolean = false): void {
    const extendedItem = item as TodoItemExtended;
    const isNewItem = extendedItem.id === 0;

    if (!extendedItem.title?.trim()) {
      this.deleteItem(extendedItem);
      return;
    }

    if (extendedItem.id === 0) {
      const createCommand = new CreateTodoItemCommand({
        listId: this.selectedList.id,
        title: extendedItem.title,
        backgroundColor: extendedItem.backgroundColor
      });

      this.itemsClient
        .create(createCommand)
        .subscribe(
          result => {
            extendedItem.id = result;
            // Update the item in the list
            const itemInList = this.selectedList.items.find(i => i === item);
            if (itemInList) {
              itemInList.id = result;
            }
          },
          error => console.error(error)
        );
    } else {
      const updateCommand = new UpdateTodoItemCommand({
        id: extendedItem.id,
        title: extendedItem.title,
        done: extendedItem.done,
        backgroundColor: extendedItem.backgroundColor
      });

      this.itemsClient.update(extendedItem.id, updateCommand).subscribe(
        () => console.log('Update succeeded.'),
        error => console.error(error)
      );
    }

    this.selectedItem = null;

    if (isNewItem && pressedEnter) {
      setTimeout(() => this.addItem(), 250);
    }
  }

  deleteItem(item: any, countDown?: boolean) {
    const extendedItem = item as TodoItemExtended;

    if (countDown) {
      if (this.deleting) {
        this.stopDeleteCountDown();
        return;
      }
      this.deleteCountDown = 3;
      this.deleting = true;
      this.deleteCountDownInterval = setInterval(() => {
        if (this.deleting && --this.deleteCountDown <= 0) {
          this.deleteItem(item, false);
        }
      }, 1000);
      return;
    }

    this.deleting = false;
    if (this.itemDetailsModalRef) {
      this.itemDetailsModalRef.hide();
    }

    if (extendedItem.id === 0) {
      const itemIndex = this.selectedList.items.findIndex(i => i === item);
      if (itemIndex >= 0) {
        this.selectedList.items.splice(itemIndex, 1);
      }
    } else {
      this.itemsClient.delete(extendedItem.id!).subscribe(
        () => {
          this.selectedList.items = this.selectedList.items.filter(
            t => t.id !== extendedItem.id
          );
        },
        error => console.error(error)
      );
    }
  }

  stopDeleteCountDown() {
    clearInterval(this.deleteCountDownInterval);
    this.deleteCountDown = 0;
    this.deleting = false;
  }

  changeItemColor(item: any, color: string): void {
    // Prevent default link behavior
    event?.preventDefault();

    const extendedItem = item as TodoItemExtended;
    // Update the item's background color
    extendedItem.backgroundColor = color;

    // Close the color picker
    this.selectedColorItem = null;

    // Call the existing updateItem method to save to the backend
    this.updateItem(extendedItem);
  }

  // Tag Management Event Handlers
  onTagCreated(tag: TagDto): void {
    console.log('Tag created:', tag);
  }

  onTagDeleted(tagId: number): void {
    console.log('Tag deleted:', tagId);
  }

  onItemTagsUpdated(tags: TagDto[], item: any): void {
    console.log('Item tags updated:', tags, 'for item:', item);
    // Store the tags for this item locally
    this.itemTags.set(item.id, tags);
  }

  // Filtering methods
  onFilterChanged(filter: FilterState): void {
    this.currentFilter = filter;
  }

  // Update the getFilteredItems method
  getFilteredItems(): any[] {
    if (!this.selectedList?.items) {
      return [];
    }

    let filteredItems = [...this.selectedList.items];

    // Apply search filter
    if (this.currentFilter.searchTerm) {
      const searchTerm = this.currentFilter.searchTerm.toLowerCase();
      filteredItems = filteredItems.filter(item =>
        item.title?.toLowerCase().includes(searchTerm)
      );
    }

    // Apply tag filter
    if (this.currentFilter.selectedTagIds.length > 0) {
      filteredItems = filteredItems.filter(item => {
        const itemTagList = this.itemTags.get(item.id) || [];
        const itemTagIds = itemTagList.map(tag => tag.id);

        // Check if item has ANY of the selected tags
        return this.currentFilter.selectedTagIds.some(selectedTagId =>
          itemTagIds.includes(selectedTagId)
        );
      });
    }

    return filteredItems;
  }

  getItemTags(itemId: number): TagDto[] {
    return this.itemTags.get(itemId) || [];
  }

}
