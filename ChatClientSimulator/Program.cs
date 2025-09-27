using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Starting Chat Client.......");

Console.Write("Enter Your Token : ");
var userId = Console.ReadLine();


var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5080/ChatHub",  //URL of host : https://socialmediaapplication.runasp.net/ChatHub
    options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(userId);
    })
    .WithAutomaticReconnect()
    .Build();

// Call back functions

connection.On<object>("ReceiveMessage", (message) =>
{
    Console.WriteLine($"[Client {userId}] New message: {message}");
});

connection.On<int, string, bool>("Typing", (chatId, senderId, isTyping) =>
{
    Console.WriteLine($"[Client {userId}] {senderId} is {(isTyping ? "typing" : "not typing")} in chat {chatId}...");
});

connection.On<int>("MessageSeen", (messageId) =>
{
    Console.WriteLine($"[Client {userId}] Message {messageId} was seen!");
});

connection.On<string, bool>("BeingOnline", (otherUserId, isOnline) =>
{
    Console.WriteLine($"[Client {userId}] User {otherUserId} is now {(isOnline ? "online" : "offline")}");
});

connection.On<int, string>("GroupNameUpdated", (groupId, name) =>
{
    Console.WriteLine($"[Client {userId}] Group {groupId} renamed to: {name}");
});

connection.On<int, string>("GroupPictureUpdated", (groupId, pictureUrl) =>
{
    Console.WriteLine($"[Client {userId}] Group {groupId} picture updated: {pictureUrl}");
});

connection.On<int, string>("NewMemberAdded", (groupId, newUserId) =>
{
    Console.WriteLine($"[Client {userId}] User {newUserId} added to group {groupId}");
});

connection.On<int, string>("MemberRemoved", (groupId, removedUserId) =>
{
    Console.WriteLine($"[Client {userId}] User {removedUserId} removed from group {groupId}");
});

connection.On<int, string>("MemberLeft", (groupId, leftUserId) =>
{
    Console.WriteLine($"[Client {userId}] User {leftUserId} left group {groupId}");
});

connection.On<int>("MessageDeleted", (messageId) =>
{
    Console.WriteLine($"[Client {userId}] Message {messageId} was deleted.");
});


try
{
    //start connection
    await connection.StartAsync();
    Console.WriteLine($"[Client {userId}] Connected to Hub!");

    while (true)
    {
        Console.WriteLine("\nChoose action:");
        Console.WriteLine("1. Send Message");
        Console.WriteLine("2. Typing");
        Console.WriteLine("3. Mark Seen");
        Console.WriteLine("4. Update Group Name");
        Console.WriteLine("5. Update Group Picture");
        Console.WriteLine("6. Leave Group");
        Console.WriteLine("7. Add Member to Group");
        Console.WriteLine("8. Remove Member from Group");
        Console.WriteLine("9. Delete Message");
        Console.WriteLine("10. Exit");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                Console.Write("ChatId: ");
                int chatId = int.Parse(Console.ReadLine()!);

                Console.Write("IsGroup (true/false): ");
                bool isGroup = bool.Parse(Console.ReadLine()!);

                Console.Write("Message: ");
                string msg = Console.ReadLine()!;

                await connection.InvokeAsync("sendMessage", chatId, isGroup, msg, null);
                break;

            case "2":
                Console.Write("ChatId: ");
                int tchatId = int.Parse(Console.ReadLine()!);

                Console.Write("IsGroup (true/false): ");
                bool tIsGroup = bool.Parse(Console.ReadLine()!);

                Console.Write("IsTyping (true/false): ");
                bool isTyping = bool.Parse(Console.ReadLine()!);

                await connection.InvokeAsync("typingStatus", tchatId, tIsGroup, isTyping);
                break;

            case "3":
                Console.Write("MessageId: ");
                int messageId = int.Parse(Console.ReadLine()!);

                Console.Write("IsGroup (true/false): ");
                bool sIsGroup = bool.Parse(Console.ReadLine()!);

                await connection.InvokeAsync("markSeen", messageId, sIsGroup);
                break;

            case "4":
                Console.Write("GroupId: ");
                int gId = int.Parse(Console.ReadLine()!);

                Console.Write("New Name: ");
                string newName = Console.ReadLine()!;

                await connection.InvokeAsync("updateGroupName", gId, newName);
                break;

            case "5":
                Console.Write("GroupId: ");
                int gpId = int.Parse(Console.ReadLine()!);

                Console.WriteLine("(Skipping actual picture upload in console client)");
                await connection.InvokeAsync("updateGroupPicture", gpId, null);
                break;

            case "6":
                Console.Write("GroupId: ");
                int lgId = int.Parse(Console.ReadLine()!);

                Console.Write("MemberId: ");
                int memberId = int.Parse(Console.ReadLine()!);

                await connection.InvokeAsync("leaveGroup", memberId, lgId);
                break;

            case "7":
                Console.Write("GroupId: ");
                int agId = int.Parse(Console.ReadLine()!);

                Console.Write("UserId to add: ");
                string addUserId = Console.ReadLine()!;

                await connection.InvokeAsync("addMemberToGroup", agId, addUserId);
                break;

            case "8":
                Console.Write("GroupId: ");
                int rgId = int.Parse(Console.ReadLine()!);

                Console.Write("MemberId: ");
                int rmemberId = int.Parse(Console.ReadLine()!);

                Console.Write("UserId: ");
                string rUserId = Console.ReadLine()!;

                await connection.InvokeAsync("removeMemberFromGroup", rgId, rmemberId, rUserId);
                break;

            case "9":
                Console.Write("MessageId: ");
                int dMsgId = int.Parse(Console.ReadLine()!);

                Console.Write("IsGroup (true/false): ");
                bool dIsGroup = bool.Parse(Console.ReadLine()!);

                await connection.InvokeAsync("DeleteMessage", dMsgId, dIsGroup);
                break;

            case "10":
                await connection.StopAsync();
                return;

            default:
                Console.WriteLine("❌ Invalid choice!");
                break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[Client {userId}] Error: " + ex.Message);
}
