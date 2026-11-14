using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FpvBattle.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Models : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Models", x => x.Id); });

            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (1, 'Gravity 250');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (2, 'Nighthawk');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (3, 'Horizon 250');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (4, 'Gravity 280');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (5, 'Lil Bastard');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (6, 'Krieger 200');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (7, 'TSX 250');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (8, 'High Wind X5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (9, 'Speed Addict 210R');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (10, 'ZMR 250');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (11, 'Mako');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (12, 'KC250');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (13, 'Rotor Evolution X1');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (14, 'Lisam 210');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (15, 'Ragg-E 200H');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (16, 'TBS Vendetta');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (17, 'QQ 190');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (18, 'Inductrix');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (19, 'UVify Draco');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (20, 'FloRotors Roosh5L');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (21, 'Acrobot TruX-R 197');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (22, 'Hornet');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (23, 'Rooster');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (24, 'Armattan Chameleon');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (25, 'TBS Oblivion');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (26, 'Neato Fastback');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (27, 'Mode 2 Ghost');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (28, 'Karearea Talon');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (29, 'Flynoceros Aether');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (30, 'ButterFly EVO');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (31, 'Dreadnought');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (32, 'DRP Mib 5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (33, 'Carbix Zero');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (34, 'Panda Cavity');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (35, 'Arch Angle Blue');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (36, 'Arch Angel Orange');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (37, 'AttoFPV Photon');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (38, 'Velocity');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (39, 'AstroX SL5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (40, 'BetaFPV 75x');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (41, 'Strix Screech');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (42, 'Source X');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (43, 'Cannonball 800');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (44, 'Ostrich Air');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (45, 'Broadsword');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (46, 'SlamNasty');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (47, 'Tiny Hawk');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (48, 'CrazyBee Micro');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (49, 'Carolina XC');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (50, 'Yakuza');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (51, 'iXC13');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (52, 'Mobula 6');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (53, 'Meteor 65');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (54, 'Beebrain');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (55, 'TBS Spec');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (56, 'AcroBee');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (57, 'ZeroGrav');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (58, 'Singularitum V5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (59, 'Five33 Switchback');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (60, 'Solleva');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (61, 'Lethal Conception');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (62, 'AcroBee PMB');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (63, 'BMS JS-1');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (64, 'Diatone GTB339');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (65, 'Dominator');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (66, 'Twig XL 3');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (67, 'HX115');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (68, 'Ossa');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (69, 'Moon Goat');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (70, 'Mad Lite');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (71, 'Shocker');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (72, 'Nazgul XL5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (73, 'Roma F5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (74, 'Junior Racer');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (75, 'SkyX Killer');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (76, 'Baby Hawk');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (77, 'Tazmanian');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (78, 'D Rock');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (79, 'PantherMM');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (80, 'DGI ');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (81, 'NanoHawk');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (82, 'Armattan Badger');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (83, 'Lumenier QAV-S');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (85, 'FPVCycle Glide');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (86, 'Armattan Marmotte');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (87, 'TBS Source One');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (88, 'Cidora');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (89, 'AOS 5.5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (90, 'Chief');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (91, 'JuiceMode');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (92, 'SniperX');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (93, 'FlipMode');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (94, 'Thor X');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (95, 'MAD Open');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (96, 'Switchback Zero');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (97, 'Fox Racing V1');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (98, 'Open Racer');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (99, 'Source One 7');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (100, 'Kauri X');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (101, 'Project399 PRIG');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (102, 'Tiny Trainer');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (103, 'Tiny Hawk 2');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (104, 'DinDrones Typhon');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (105, 'Five33 Spec 7');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (106, 'Heavy Metal');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (107, 'Super 70');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (108, 'LightSwitch');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (109, 'Mid Mount');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (110, 'Slicer SR5');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (111, 'Fox Crooked');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (112, 'Race Whoop');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (113, 'Hummingbird V3');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (114, 'Cetus Pro');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (115, 'Cetus X');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (116, 'Mobula 6 2024');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (117, '7 Wonders');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (118, 'MGPSpec');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (119, 'Air 65 Racing');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (120, 'Air 65 Freestyle');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (121, 'Hummingbird RS');");
            migrationBuilder.Sql("INSERT INTO Models (Id, Name) VALUES (122, 'OZR-5X');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Models");
        }
    }
}
