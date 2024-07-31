﻿using Sandbox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NPC : Component, IInteractable
{

	private enum AnimationBehaviour
	{
		Default,
		Wave,
		Cheer
	}

	public Player Owner { get; set; } = null;

	[Property] public SkinnedModelRenderer Renderer { get; set; }

	[Property] public Face Face { get; set; }

	public Color Color { get; private set; }

	[Property] GameObject LookAt { get; set; }

	private Transform? ForwardReference => Renderer.GetAttachment( "forward_reference" );

	public bool IsInteractableBy( Player player ) => true;

	public void OnMouseEnter( Guid playerId )
	{
		Renderer.Set( "b_big_head", true );
	}

	public void OnMouseExit( Guid playerId )
	{
		Renderer.Set( "b_big_head", false );
	}

	public void OnInteract( Guid playerId )
	{
		Log.Info( "Interacted with " + GameObject.Name );
		Renderer.Set( "e_behaviour", (int)AnimationBehaviour.Wave );
	}

	public NPC() { }

	protected override void OnStart()
	{
		base.OnStart();	

		LookAt = Scene.Camera.GameObject;

		Renderer.Set( "b_walking", true );

		// TODO: This should not be done from within NPC.
		Spawn();


		if (IsProxy) { return; }

		// TODO: Color needs to be networked.
		SetColor( ColorX.MiiColors.GetRandom() );

	}

	private void Spawn()
	{
		GameObject.Transform.Position = FindSpawnLocation().Position;
	}

	Transform FindSpawnLocation()
	{

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			SpawnPoint sp = Random.Shared.FromArray( spawnPoints );
			Transform t = sp.Transform.World;
			sp.Destroy();
			return t;
		}

		//
		// Failing that, spawn where we are
		//
		return Transform.World;
	}

	[Broadcast]
	public void SetColor( Color color )
	{
		Face.SetColor( color );
		Renderer.Tint = color;
	}

	public void GetPosessedBy(Player owner)
	{
		this.Owner = owner;
		Network.AssignOwnership( owner.Network.OwnerConnection );

		Log.Info($"{owner.Network.OwnerConnection.DisplayName} posessed npc");
		GameObject.Name = $"NPC ({owner.Network.OwnerConnection.DisplayName})";

		Face.Load();

	}

	protected override void OnUpdate()
	{
		if(LookAt != null) {
			LookAtPosition( Scene.Camera.Transform.Position );	
		}

		Vector3 fwd = ForwardReference?.Forward ?? 0;
		Vector3 scale =  ( ForwardReference?.Scale ?? 1 );
		Face.Transform.Position = (ForwardReference?.Position ?? 0) + (fwd.Normal * 14f * scale);
		Face.Transform.Scale = 11 * scale;

	}

	private void LookAtPosition( Vector3 pos )
	{
		Vector3 from = ForwardReference?.Position ?? 0;
		Vector3 dir = (pos - from);
		Renderer.SetLookDirection( "aim_head", dir );

	}

}
