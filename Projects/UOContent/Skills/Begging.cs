using Server.Accounting;
using Server.Commands;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.SkillHandlers
{
    public static class Begging
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Begging].Callback = OnUse;
        }

        public static TimeSpan OnUse(Mobile m)
        {
            m.RevealingAction();

            m.Target = new InternalTarget();
            m.RevealingAction();

            m.SendLocalizedMessage(500397); // To whom do you wish to grovel?

            return TimeSpan.FromSeconds(30.0);
        }

        private class InternalTarget : Target
        {
            public InternalTarget() : base(12, false, TargetFlags.None)
            {
            }

            protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
            {
                from.NextSkillTime = Core.TickCount;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is not Mobile targ)
                {
                    from.SendLocalizedMessage(500399); // There is little chance of getting money from that!
                }
                else if (targ.Player) // We can't beg from players
                {
                    from.SendLocalizedMessage(500398); // Perhaps just asking would work better.
                }
                else if (!targ.Body.IsHuman) // Make sure the NPC is human
                {
                    from.SendLocalizedMessage(500399); // There is little chance of getting money from that!
                }
                else if (!from.InRange(targ, 2))
                {
                    if (!targ.Female)
                    {
                        from.SendLocalizedMessage(500401); // You are too far away to beg from him.
                    }
                    else
                    {
                        from.SendLocalizedMessage(500402); // You are too far away to beg from her.
                    }
                }
                // If we're on a mount, who would give us money? TODO: guessed it's removed since ML
                else if (!Core.ML && from.Mounted)
                {
                    from.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
                }
                else
                {
                    from.RevealingAction();

                    // Face each other
                    from.Direction = from.GetDirectionTo(targ);
                    targ.Direction = targ.GetDirectionTo(from);

                    from.Animate(32, 5, 1, true, false, 0); // Bow

                    new InternalTimer(from, targ).Start();
                    return;
                }

                from.NextSkillTime = Core.TickCount;
            }

            private class InternalTimer : Timer
            {
                private readonly Mobile _from;
                private readonly Mobile _target;

                public InternalTimer(Mobile from, Mobile target) : base(TimeSpan.FromSeconds(2.0))
                {
                    _from = from;
                    _target = target;
                }

                protected override void OnTick()
                {
                    ProcessBegging();

                    const int TargeterCooldown = 30000; // 30s
                    const int SkillCooldown = 10000;    // 10s

                    // Calculate how much time has passed since the targeter was opened
                    int ticksSinceTargeter = (int)(Core.TickCount - (_from.NextSkillTime - TargeterCooldown));
                    int remainingCooldown = Math.Max(0, SkillCooldown - ticksSinceTargeter);
                    _from.NextSkillTime = Core.TickCount + remainingCooldown;
                }

                public void ProcessBegging()
                {
                    var calculateFameKarmaPercentage = CalculateFameKarmaPercentage(_from.Fame, _from.Karma);

                    //var calculateFameKarmaPercentage = CalculateFameKarmaPercentage(0, 0);     // => 28
                    //var calculateFameKarmaPercentage = CalculateFameKarmaPercentage(3000, 10000);    // => ~75
                    //var calculateFameKarmaPercentage = CalculateFameKarmaPercentage(12000, -10000);    // => ~30.0
                    //var calculateFameKarmaPercentage = CalculateFameKarmaPercentage(15000, 15000);  // => ~100
                    //var calculateFameKarmaPercentage = CalculateFameKarmaPercentage(0, -15000);  // => 0



                    var theirPack = _target.Backpack;

                    theirPack.AddItem(new TreasureMap(1, Map.Felucca));
                    theirPack.AddItem(new TreasureMap(2, Map.Felucca));
                    theirPack.AddItem(new TreasureMap(3, Map.Felucca));


                    //var rndLoot = Loot.Construct(Loot.ArmorTypes);
                    //theirPack.AddItem(rndLoot);
                    //rndLoot = Loot.Construct(Loot.BeverageTypes);
                    //theirPack.AddItem(rndLoot);
                    //rndLoot = Loot.Construct(Loot.FoodTypes);
                    //theirPack.AddItem(rndLoot);

                    if (calculateFameKarmaPercentage <= 20)
                    {
                        var values = Enum.GetValues(typeof(BeverageType));
                        var randomType = (BeverageType)values.GetValue(new System.Random().Next(values.Length));
                        theirPack.AddItem(new BeverageBottle(randomType));
                    }
                    else if (calculateFameKarmaPercentage <= 40)
                    {
                    }
                    else if (calculateFameKarmaPercentage <= 60)
                    {
                    }
                    else if (calculateFameKarmaPercentage <= 80)
                    {
                    }
                    else if (calculateFameKarmaPercentage <= 100)
                    {
                    }


                    //var badKarmaChance = 0.5 - (double)_from.Karma / 8570;

                    //if (theirPack == null)
                    //{
                    //    _from.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
                    //}
                    //else if (_from.Karma < 0 && badKarmaChance > Utility.RandomDouble())
                    //{
                    //    // Thou dost not look trustworthy... no gold for thee today!
                    //    _target.PublicOverheadMessage(MessageType.Regular, _target.SpeechHue, 500406);
                    //}
                    //else if (_from.CheckTargetSkill(SkillName.Begging, _target, 0.0, 100.0))
                    //{
                    //    var toConsume = theirPack.GetAmount(typeof(Gold)) / 10;
                    //    var max = Math.Clamp(10 + _from.Fame / 2500, 10, 14);

                    //    if (toConsume > max)
                    //    {
                    //        toConsume = max;
                    //    }

                    //    if (toConsume > 0)
                    //    {
                    //        var consumed = theirPack.ConsumeUpTo(typeof(Gold), toConsume);

                    //        if (consumed > 0)
                    //        {
                    //            // I feel sorry for thee...
                    //            _target.PublicOverheadMessage(MessageType.Regular, _target.SpeechHue, 500405);

                    //            var gold = new Gold(consumed);

                    //            _from.AddToBackpack(gold);
                    //            _from.PlaySound(gold.GetDropSound());

                    //            if (_from.Karma > -3000)
                    //            {
                    //                var toLose = _from.Karma + 3000;

                    //                if (toLose > 40)
                    //                {
                    //                    toLose = 40;
                    //                }

                    //                Titles.AwardKarma(_from, -toLose, true);
                    //            }
                    //        }
                    //        else
                    //        {
                    //            // I have not enough money to give thee any!
                    //            _target.PublicOverheadMessage(MessageType.Regular, _target.SpeechHue, 500407);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // I have not enough money to give thee any!
                    //        _target.PublicOverheadMessage(MessageType.Regular, _target.SpeechHue, 500407);
                    //    }
                    //}
                    //else
                    //{
                    //    _target.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
                    //}
                }

                private Item CreateRandomFood() =>
                    Utility.Random(10) switch
                    {
                        0 => new Grapes(),
                        1 => new Ham(),
                        2 => new CheeseWedge(),
                        3 => new Muffins(),
                        4 => new FishSteak(),
                        5 => new Ribs(),
                        6 => new CookedBird(),
                        7 => new Sausage(),
                        8 => new Apple(),
                        9 => new Peach(),
                        _ => null
                    };

                private double CalculateFameKarmaPercentage(int fame, int karma)
                {
                    // Clamp inputs to valid ranges
                    fame = Math.Clamp(fame, Titles.MinFame, Titles.MaxFame);
                    karma = Math.Clamp(karma, Titles.MinKarma, Titles.MaxKarma);

                    // Dynamically extract fame thresholds from fameEntries
                    int[] fameThresholds = Titles.fameEntries.Select(entry => entry.m_Fame).ToArray();

                    // Calculate fame level (0 to 5)
                    int fameLevel = 0;
                    for (int i = 0; i < fameThresholds.Length; i++)
                    {
                        if (fame < fameThresholds[i])
                        {
                            break;
                        }
                        fameLevel = i + 1;
                    }

                    // Get karma thresholds from the FameEntry corresponding to fameLevel
                    var fameEntry = fameLevel > 0 ? Titles.fameEntries[Math.Min(fameLevel - 1, Titles.fameEntries.Length - 1)] :
                                    Titles.fameEntries.Length > 0 ? Titles.fameEntries[0] : Titles.fameEntries[Titles.fameEntries.Length - 1];
                    var karmaEntries = fameEntry.m_Karma;

                    // Calculate karma level (0 to 10)
                    int karmaLevel = 0;
                    for (int i = karmaEntries.Length - 1; i >= 0; i--)
                    {
                        if (karma >= karmaEntries[i].m_Karma)
                        {
                            karmaLevel = i;
                            break;
                        }
                    }

                    // Compute weighted percentages
                    double famePercentage = (fameLevel / 5.0) * 30; // Max 30%
                    double karmaPercentage = (karmaLevel / 10.0) * 70; // Max 70%

                    // Return total percentage
                    return famePercentage + karmaPercentage;
                }
            }
        }
    }
}
