using NUnit.Framework;
using FluentAssertions;
using Todo_App.Application.TodoLists.Commands.SoftDeleteTodoList;
using Todo_App.Domain.Entities;

namespace Todo_App.Application.UnitTests.TodoLists.Commands;

[TestFixture]
public class SoftDeleteTodoListCommandTests
{
    [Test]
    public void SoftDeleteTodoListCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var id = 456;

        // Act
        var command = new SoftDeleteTodoListCommand { Id = id };

        // Assert
        command.Id.Should().Be(id);
    }

    [Test]
    public void TodoList_ShouldImplementISoftDeletable()
    {
        // Arrange & Act
        var todoList = new TodoList();

        // Assert
        todoList.IsDeleted.Should().BeFalse();
        todoList.DeletedAt.Should().BeNull();

        // Test soft delete properties can be set
        todoList.IsDeleted = true;
        todoList.DeletedAt = DateTime.UtcNow;

        todoList.IsDeleted.Should().BeTrue();
        todoList.DeletedAt.Should().NotBeNull();
    }

    [Test]
    public void TodoList_SoftDeleteProperties_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        var todoList = new TodoList
        {
            Title = "Test List"
        };

        // Assert
        todoList.IsDeleted.Should().BeFalse("new lists should not be deleted by default");
        todoList.DeletedAt.Should().BeNull("new lists should not have a deletion timestamp");
    }

    [Test]
    public void TodoList_WithItems_ShouldSupportSoftDelete()
    {
        // Arrange
        var todoList = new TodoList { Title = "Test List" };
        var item1 = new TodoItem { Title = "Item 1" };
        var item2 = new TodoItem { Title = "Item 2" };

        todoList.Items.Add(item1);
        todoList.Items.Add(item2);

        // Act
        todoList.IsDeleted = true;
        todoList.DeletedAt = DateTime.UtcNow;

        // Assert
        todoList.IsDeleted.Should().BeTrue();
        todoList.DeletedAt.Should().NotBeNull();
        todoList.Items.Should().HaveCount(2);
    }
}