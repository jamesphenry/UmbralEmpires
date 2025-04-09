// --- File: Player.cs ---
namespace UmbralEmpires.Core.Gameplay;

public class Player
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int Credits { get; private set; }

    public Player(Guid id, string name = "Player", int startingCredits = 500)
    {
        if (id == Guid.Empty) throw new ArgumentException("Player ID cannot be empty.", nameof(id));
        Id = id;
        Name = string.IsNullOrWhiteSpace(name) ? $"Player_{id.ToString().Substring(0, 4)}" : name;
        Credits = startingCredits >= 0 ? startingCredits : 0;
    }

    public void AddCredits(int amount)
    {
        if (amount < 0) return; // Or throw? For now, just ignore negative adds.
        Credits += amount;
        // Consider adding Domain Event: PlayerCreditsChanged(Id, Credits)
    }

    public bool SpendCredits(int amount)
    {
        if (amount <= 0) return false; // Cannot spend non-positive
        if (Credits >= amount)
        {
            Credits -= amount;
            // Consider adding Domain Event: PlayerCreditsChanged(Id, Credits)
            return true;
        }
        return false; // Insufficient credits
    }

    public void SetName(string newName) // Allow renaming
    {
        if (!string.IsNullOrWhiteSpace(newName)) Name = newName;
    }

    // EF Core constructor
    private Player() { Name = null!; }
}