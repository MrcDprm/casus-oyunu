using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace casus_oyunu.Migrations
{
    /// <inheritdoc />
    public partial class asddsacvcvc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRooms_Users_CreatorId",
                table: "GameRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserThemes_Users_UserId",
                table: "UserThemes");

            migrationBuilder.DropTable(
                name: "TournamentMatches");

            migrationBuilder.DropTable(
                name: "TournamentParticipants");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_GameRooms_RoomCode",
                table: "GameRooms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserThemes",
                table: "UserThemes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscriptions",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_Type",
                table: "UserSubscriptions");

            migrationBuilder.RenameTable(
                name: "UserThemes",
                newName: "UserTheme");

            migrationBuilder.RenameTable(
                name: "UserSubscriptions",
                newName: "UserSubscription");

            migrationBuilder.RenameIndex(
                name: "IX_UserThemes_UserId",
                table: "UserTheme",
                newName: "IX_UserTheme_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscription",
                newName: "IX_UserSubscription_UserId");

            migrationBuilder.AddColumn<int>(
                name: "GameRoomId1",
                table: "RoomParticipants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "RoomParticipants",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserTheme",
                table: "UserTheme",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscription",
                table: "UserSubscription",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameRoomId = table.Column<int>(type: "int", nullable: false),
                    SenderParticipantId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_RoomParticipants_SenderParticipantId",
                        column: x => x.SenderParticipantId,
                        principalTable: "RoomParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameRoomId = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    CurrentQuestion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentQuestionerId = table.Column<int>(type: "int", nullable: true),
                    CurrentAnswererId = table.Column<int>(type: "int", nullable: true),
                    SpyParticipantId = table.Column<int>(type: "int", nullable: true),
                    VotingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    GameFinished = table.Column<bool>(type: "bit", nullable: false),
                    Winner = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GameRoomId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameSessions_GameRooms_GameRoomId1",
                        column: x => x.GameRoomId1,
                        principalTable: "GameRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlayerPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomParticipantId = table.Column<int>(type: "int", nullable: false),
                    X = table.Column<float>(type: "real", nullable: false),
                    Y = table.Column<float>(type: "real", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerPositions_RoomParticipants_RoomParticipantId",
                        column: x => x.RoomParticipantId,
                        principalTable: "RoomParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomParticipants_GameRoomId1",
                table: "RoomParticipants",
                column: "GameRoomId1");

            migrationBuilder.CreateIndex(
                name: "IX_RoomParticipants_UserId1",
                table: "RoomParticipants",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_GameRoomId",
                table: "ChatMessages",
                column: "GameRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderParticipantId",
                table: "ChatMessages",
                column: "SenderParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_GameRoomId",
                table: "GameSessions",
                column: "GameRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_GameRoomId1",
                table: "GameSessions",
                column: "GameRoomId1");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPositions_RoomParticipantId",
                table: "PlayerPositions",
                column: "RoomParticipantId");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRooms_Users_CreatorId",
                table: "GameRooms",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_GameRooms_GameRoomId1",
                table: "RoomParticipants",
                column: "GameRoomId1",
                principalTable: "GameRooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_Users_UserId1",
                table: "RoomParticipants",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscription_Users_UserId",
                table: "UserSubscription",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTheme_Users_UserId",
                table: "UserTheme",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameRooms_Users_CreatorId",
                table: "GameRooms");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_GameRooms_GameRoomId1",
                table: "RoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_Users_UserId1",
                table: "RoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscription_Users_UserId",
                table: "UserSubscription");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTheme_Users_UserId",
                table: "UserTheme");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "PlayerPositions");

            migrationBuilder.DropIndex(
                name: "IX_RoomParticipants_GameRoomId1",
                table: "RoomParticipants");

            migrationBuilder.DropIndex(
                name: "IX_RoomParticipants_UserId1",
                table: "RoomParticipants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserTheme",
                table: "UserTheme");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscription",
                table: "UserSubscription");

            migrationBuilder.DropColumn(
                name: "GameRoomId1",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "RoomParticipants");

            migrationBuilder.RenameTable(
                name: "UserTheme",
                newName: "UserThemes");

            migrationBuilder.RenameTable(
                name: "UserSubscription",
                newName: "UserSubscriptions");

            migrationBuilder.RenameIndex(
                name: "IX_UserTheme_UserId",
                table: "UserThemes",
                newName: "IX_UserThemes_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscription_UserId",
                table: "UserSubscriptions",
                newName: "IX_UserSubscriptions_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserThemes",
                table: "UserThemes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscriptions",
                table: "UserSubscriptions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EntryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPremiumOnly = table.Column<bool>(type: "bit", nullable: false),
                    MaxParticipants = table.Column<int>(type: "int", nullable: false),
                    MinParticipants = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrizePool = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RegistrationDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tournaments_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TournamentMatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GameRoomId = table.Column<int>(type: "int", nullable: true),
                    Player1Id = table.Column<int>(type: "int", nullable: true),
                    Player2Id = table.Column<int>(type: "int", nullable: true),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    WinnerId = table.Column<int>(type: "int", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Round = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_GameRooms_GameRoomId",
                        column: x => x.GameRoomId,
                        principalTable: "GameRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Users_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Users_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentMatches_Users_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TournamentParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FinalRank = table.Column<int>(type: "int", nullable: true),
                    HasPaid = table.Column<bool>(type: "bit", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrizeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentParticipants_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameRooms_RoomCode",
                table: "GameRooms",
                column: "RoomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_Type",
                table: "UserSubscriptions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_GameRoomId",
                table: "TournamentMatches",
                column: "GameRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Player1Id",
                table: "TournamentMatches",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_Player2Id",
                table: "TournamentMatches",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_TournamentId",
                table: "TournamentMatches",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_WinnerId",
                table: "TournamentMatches",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentParticipants_TournamentId",
                table: "TournamentParticipants",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentParticipants_UserId",
                table: "TournamentParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_CreatorId",
                table: "Tournaments",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_StartDate",
                table: "Tournaments",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_Status",
                table: "Tournaments",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_GameRooms_Users_CreatorId",
                table: "GameRooms",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserThemes_Users_UserId",
                table: "UserThemes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
