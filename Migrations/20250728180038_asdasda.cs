using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace casus_oyunu.Migrations
{
    /// <inheritdoc />
    public partial class asdasda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_GameRooms_GameRoomId",
                table: "RoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                table: "RoomParticipants");

            migrationBuilder.AddColumn<int>(
                name: "SelectedDuration",
                table: "GameRooms",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_GameRooms_GameRoomId",
                table: "RoomParticipants",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                table: "RoomParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_GameRooms_GameRoomId",
                table: "RoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "SelectedDuration",
                table: "GameRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_GameRooms_GameRoomId",
                table: "RoomParticipants",
                column: "GameRoomId",
                principalTable: "GameRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                table: "RoomParticipants",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
