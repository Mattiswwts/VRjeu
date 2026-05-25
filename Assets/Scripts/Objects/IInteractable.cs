namespace ActionGame.Objects
{
    /// <summary>
    /// Interface pour tout objet interactable par l'Acteur.
    /// interactor peut être null en VR (l'objet est tenu par le contrôleur XRI directement).
    /// </summary>
    public interface IInteractable
    {
        void Interact(ActionGame.Player.ActorController actor);
    }
}
