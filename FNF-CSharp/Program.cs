namespace FNF_CSharp;

public static class Program {
    public static void Main() {
        using var game = new FnfGame();
        game.Run();
    }
}
