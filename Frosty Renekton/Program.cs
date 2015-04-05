using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
namespace ElRenekton
{
	class Program
	{
		public static bool hasCastedEOnce = false;
		public static Items.Item TIA;
		public static Items.Item HYD;
		public static string ChampName = "Renekton";
		public static Orbwalking.Orbwalker Orbwalker;
		public static Obj_AI_Base Player;
		public static Spell Q, W, E, R, AA;
		public static Menu RenektonWrapper;

		static void Main(string[] args)
		{
			CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
		}

		static void Game_OnGameLoad(EventArgs args)
		{
			Player = ObjectManager.Player;
			TIA = new Items.Item(3077, 400);
			HYD = new Items.Item(3074, 400);
			if (Player.BaseSkinName != ChampName)
				return;
			Q = new Spell(SpellSlot.Q, 250);
			W = new Spell(SpellSlot.W, Player.AttackRange);
			E = new Spell(SpellSlot.E, 460);
			R = new Spell(SpellSlot.R, float.MaxValue);
			E.SetSkillshot(0.5f, 50.0f, 20.0f, false, SkillshotType.SkillshotLine);
			RenektonWrapper = new Menu("ElRenekton", ChampName, true);
			RenektonWrapper.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
			Orbwalker = new Orbwalking.Orbwalker(RenektonWrapper.SubMenu("Orbwalker"));
			var ts = new Menu("Target Selector", "Target Selector");
			TargetSelector.AddToMenu(ts);
			RenektonWrapper.AddSubMenu(ts);
			RenektonWrapper.AddSubMenu(new Menu("Combo", "Combo"));
			RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("chance", "Use R at Lifepercentage?").SetValue(new Slider(40, 0, 100)));
			RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
			RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("useR", "Use R").SetValue(true));
			RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("useE2", "Use E twice").SetValue(true));
			RenektonWrapper.SubMenu("Combo").AddItem(new MenuItem("useE2Back", "Use second E to return to Start").SetValue(true));
			RenektonWrapper.AddSubMenu(new Menu("Interrupt", "Interrupt"));
			RenektonWrapper.AddSubMenu(new Menu("Harrass", "Harrass"));
			RenektonWrapper.AddSubMenu(new Menu("Farm", "Farm"));
			RenektonWrapper.AddSubMenu(new Menu("Draw", "Drawing"));
			RenektonWrapper.SubMenu("Draw").AddItem(new MenuItem("DrawActive", "Enable drawings").SetValue(new KeyBind("L".ToCharArray()[0], KeyBindType.Toggle)));
			RenektonWrapper.SubMenu("Harrass").AddItem(new MenuItem("HarrassActive", "Harrass").SetValue(new KeyBind("X".ToCharArray()[0], KeyBindType.Press)));
			RenektonWrapper.SubMenu("Farm").AddItem(new MenuItem("FarmActive", "Farm").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
			RenektonWrapper.SubMenu("Farm").AddItem(new MenuItem("useqf", "Use Q to Farm").SetValue(true));
			RenektonWrapper.AddSubMenu(new Menu("AntiGapclose", "AntiGapclose"));
			RenektonWrapper.SubMenu("AntiGapclose").AddItem(new MenuItem("useeg", "Use E to AntiGapclose").SetValue(true));
			RenektonWrapper.SubMenu("Interrupt").AddItem(new MenuItem("usew", "Use W to Interrupt").SetValue(true));
			RenektonWrapper.AddToMainMenu();
			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnUpdate += Game_OnGameUpdate;
			AntiGapcloser.OnEnemyGapcloser += AntiGapclose;
			Interrupter2.OnInterruptableTarget += OnPosibleToInterrupt;

		}

		private static void OnPosibleToInterrupt(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
		{
			if (sender.Position.Distance(Player.Position) < Player.AttackRange)
			{
				Orbwalker.SetAttack(false);
				W.Cast();
				Player.IssueOrder(GameObjectOrder.AttackUnit, sender);
				Orbwalker.SetAttack(true);
			}
		}
		static void Game_OnGameUpdate(EventArgs args)
		{


			if (RenektonWrapper.Item("ComboActive").GetValue<KeyBind>().Active)
			{
				Combo();
			}
			if (RenektonWrapper.Item("FarmActive").GetValue<KeyBind>().Active)
			{
				Farm();
			}
			if (RenektonWrapper.Item("HarrassActive").GetValue<KeyBind>().Active)
			{
				Harrass();
			}
			if (RenektonWrapper.Item("useR").GetValue<bool>() && R.IsReady() && RenektonWrapper.Item("chance").GetValue<Slider>().Value >= (Player.HealthPercent))
			{

				R.Cast();
			}
		}

		static void Drawing_OnDraw(EventArgs args)
		{
			if (RenektonWrapper.Item("DrawActive").GetValue<KeyBind>().Active)
			{
				Render.Circle.DrawCircle(Player.Position, Q.Range, Color.Azure);
				Render.Circle.DrawCircle(Player.Position, E.Range, Color.Crimson);
			}

		}

		public static void Harrass()
		{
			var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
			if (target == null)
				return;

			if (Q.IsReady())
			{
				Q.Cast();
			}
		}

		public static void Combo()
		{

			if (RenektonWrapper.Item("useE2").GetValue<bool>() && RenektonWrapper.Item("useE2Back").GetValue<bool>())
			{
				jQuerySucks(); //will be the standard combo in the feature with a more advance Logic
			}
			if(RenektonWrapper.Item("useE2").GetValue<bool>() && !RenektonWrapper.Item("useE2Back").GetValue<bool>())
			{
				jQueryisCool();
			}

			

		}

		private static void jQueryisCool()
		{
			var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
			if (target == null)
				return;

			if (target.IsValidTarget(E.Range) && E.IsReady())
			{
				E.Cast(target.Position);
				E.Cast(target.Position);

			}
			if (target.IsValidTarget(Q.Range) && Q.IsReady())
			{
				Q.Cast();

			}
			if (target.IsValidTarget(Player.AttackRange))
			{
				doAAresets(target);
			}
			
		}

		private static void jQuerySucks()
		{

			var startPos = Player.Position;
			var target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
			if (target == null)
				return;
			if(E.IsReady())
				E.Cast(target);
			if (Q.IsInRange(target) && Q.IsReady())
				Q.Cast();
			if (target.Position.Distance(Player.Position) < Player.AttackRange && W.IsReady())
				doAAresets(target);
			E.Cast(startPos);
		}

		private static void doAAresets(Obj_AI_Base target)
		{
			Orbwalker.SetAttack(false);
			Player.IssueOrder(GameObjectOrder.AttackUnit, target);
			W.Cast();
			Player.IssueOrder(GameObjectOrder.AttackUnit, target);
			HYD.Cast();
			TIA.Cast();
			Player.IssueOrder(GameObjectOrder.AttackUnit, target);
			Orbwalker.SetAttack(true);
		}



		public static void Farm()
        {
            if (RenektonWrapper.Item("useqf").GetValue<bool>())
            {
                var Rangeminions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
                if (Q.IsReady())
                {
					var minionKillCount = 0;
                    foreach (var minion in Rangeminions)
                    {
                        if (minion.IsValidTarget() && HealthPrediction.GetHealthPrediction(minion, 25) <= Q.GetDamage(minion))
                        {
                            
                           minionKillCount++;
                        }
                            
                    }
					if(minionKillCount >= 2 && Rangeminions.Count > 5)
						Q.Cast();
                        
                }
            }
        }
		public static void AntiGapclose(ActiveGapcloser gapcloser)
		{
			if (RenektonWrapper.Item("useeg").GetValue<bool>())
			{


				var target = gapcloser.Start;
				if (E.IsReady())
				{
					var distance = target.Distance(Player.Position) + E.Range - 10;
					var newPos = target.Extend(Player.Position, distance);
					E.Cast(newPos);
				}
			}
		}
	}
}


