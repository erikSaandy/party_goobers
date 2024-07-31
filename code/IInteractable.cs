using System;

public interface IInteractable
{
	public abstract bool IsInteractableBy( Player player );
	public abstract void OnInteract( Guid playerId );

	public virtual void OnMouseEnter( Guid playerId ) { }
	public virtual void OnMouseHover( Guid playerId ) { }
	public virtual void OnMouseExit( Guid playerId ) { }

}
