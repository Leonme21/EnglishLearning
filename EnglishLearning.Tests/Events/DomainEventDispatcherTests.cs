using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Events;
using EnglishLearning.Domain.ValueObjects;
using EnglishLearning.Infrastructure.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EnglishLearning.Tests.Events;

public class DomainEventDispatcherTests
{
    private class TestLevelChangedHandler : IDomainEventHandler<LevelChangedEvent>
    {
        public int CallCount { get; private set; }
        public LevelChangedEvent? LastEvent { get; private set; }

        public Task HandleAsync(LevelChangedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastEvent = domainEvent;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_ShouldInvokeRegisteredHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new TestLevelChangedHandler();
        services.AddSingleton<IDomainEventHandler<LevelChangedEvent>>(handler);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new DomainEventDispatcher(NullLogger<DomainEventDispatcher>.Instance, serviceProvider);

        var domainEvent = new LevelChangedEvent
        {
            UserId = UserId.Create(1).Value,
            PreviousLevel = NivelIngles.A1,
            NewLevel = NivelIngles.A2,
            AchievementPercentage = Percentage.Create(85).Value
        };

        // Act
        await dispatcher.DispatchAsync(domainEvent);

        // Assert
        Assert.Equal(1, handler.CallCount);
        Assert.NotNull(handler.LastEvent);
        Assert.Equal(NivelIngles.A2, handler.LastEvent.NewLevel);
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_ShouldDispatchAll()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler = new TestLevelChangedHandler();
        services.AddSingleton<IDomainEventHandler<LevelChangedEvent>>(handler);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new DomainEventDispatcher(NullLogger<DomainEventDispatcher>.Instance, serviceProvider);

        var events = new List<IDomainEvent>
        {
            new LevelChangedEvent
            {
                UserId = UserId.Create(1).Value,
                PreviousLevel = NivelIngles.A1,
                NewLevel = NivelIngles.A2,
                AchievementPercentage = Percentage.Create(80).Value
            },
            new LevelChangedEvent
            {
                UserId = UserId.Create(2).Value,
                PreviousLevel = NivelIngles.A2,
                NewLevel = NivelIngles.B1,
                AchievementPercentage = Percentage.Create(90).Value
            }
        };

        // Act
        await dispatcher.DispatchAsync(events);

        // Assert
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public void Usuario_ActualizarNivel_ShouldEmitLevelChangedEvent()
    {
        // Arrange
        var usuario = Usuario.Create(1, "Test User", "test@test.com", "hash", NivelIngles.A1).Value;

        // Act
        usuario.ActualizarNivel(NivelIngles.B1);

        // Assert
        Assert.Single(usuario.DomainEvents);
        var evt = Assert.IsType<LevelChangedEvent>(usuario.DomainEvents.First());
        Assert.Equal(NivelIngles.A1, evt.PreviousLevel);
        Assert.Equal(NivelIngles.B1, evt.NewLevel);

        usuario.ClearDomainEvents();
        Assert.Empty(usuario.DomainEvents);
    }
}
