using Godot;

namespace Grao;

// Camada por cima do mundo: desenha pessoas, neutrões e o pincel, e recebe o rato.
public partial class Overlay : Control
{
    public Main M;
    public override void _Draw() => M?.DrawOverlay(this);
    public override void _GuiInput(InputEvent e) => M?.OnWorldInput(e);
}
