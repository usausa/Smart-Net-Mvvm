namespace Smart.Mvvm.Messaging;

public sealed class MessengerTest
{
    //------------------------------------------------------------
    // Send
    //------------------------------------------------------------

    [Fact]
    public void SendLabelArgsHaveCorrectLabelAndMessageType()
    {
        var messenger = new Messenger();
        MessengerEventArgs? received = null;
        messenger.Received += (_, args) => received = args;

        messenger.Send("TestLabel");

        Assert.NotNull(received);
        Assert.Equal("TestLabel", received.Label);
        Assert.Equal(typeof(object), received.MessageType);
        Assert.Null(received.Message);
    }

    [Fact]
    public void SendEmptyLabelUsesSharedEmptyEventArgs()
    {
        var messenger = new Messenger();
        MessengerEventArgs? first = null;
        MessengerEventArgs? second = null;

        void Handler1(object? sender, MessengerEventArgs args) => first = args;
        void Handler2(object? sender, MessengerEventArgs args) => second = args;

        messenger.Received += Handler1;
        messenger.Send(string.Empty);
        messenger.Received -= Handler1;

        messenger.Received += Handler2;
        messenger.Send(string.Empty);
        messenger.Received -= Handler2;

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Same(first, second);
        Assert.Equal(string.Empty, first.Label);
        Assert.Equal(typeof(object), first.MessageType);
        Assert.Null(first.Message);
    }

    //------------------------------------------------------------
    // Send<T>
    //------------------------------------------------------------

    [Fact]
    public void SendMessageArgsHaveEmptyLabelAndCorrectMessageType()
    {
        var messenger = new Messenger();
        MessengerEventArgs? received = null;
        messenger.Received += (_, args) => received = args;

        // Use explicit type argument to distinguish from Send(string label)
        messenger.Send<string>("hello world");

        Assert.NotNull(received);
        Assert.Equal(string.Empty, received.Label);
        Assert.Equal(typeof(string), received.MessageType);
        Assert.Equal("hello world", received.Message);
    }

    [Fact]
    public void SendIntMessageArgsHaveCorrectMessageType()
    {
        var messenger = new Messenger();
        MessengerEventArgs? received = null;
        messenger.Received += (_, args) => received = args;

        messenger.Send(42);

        Assert.NotNull(received);
        Assert.Equal(string.Empty, received.Label);
        Assert.Equal(typeof(int), received.MessageType);
        Assert.Equal(42, received.Message);
    }

    //------------------------------------------------------------
    // Send<T>
    //------------------------------------------------------------

    [Fact]
    public void SendLabelParameterArgsHaveCorrectValues()
    {
        var messenger = new Messenger();
        MessengerEventArgs? received = null;
        messenger.Received += (_, args) => received = args;

        messenger.Send("Navigate", 99);

        Assert.NotNull(received);
        Assert.Equal("Navigate", received.Label);
        Assert.Equal(typeof(int), received.MessageType);
        Assert.Equal(99, received.Message);
    }

    //------------------------------------------------------------
    // Unsubscribe
    //------------------------------------------------------------

    [Fact]
    public void UnsubscribeAfterRemoveNoLongerReceivesMessages()
    {
        var messenger = new Messenger();
        var count = 0;

        void Handler(object? sender, MessengerEventArgs e) => count++;
        messenger.Received += Handler;
        messenger.Send("First");

        messenger.Received -= Handler;
        messenger.Send("Second");

        Assert.Equal(1, count);
    }

    //------------------------------------------------------------
    // No subscribers
    //------------------------------------------------------------

    [Fact]
    public void SendWithNoSubscribersDoesNotThrow()
    {
        var messenger = new Messenger();

        var ex1 = Record.Exception(() => messenger.Send("label"));
        var ex2 = Record.Exception(() => messenger.Send("message"));
        var ex3 = Record.Exception(() => messenger.Send("label", 1));

        Assert.Null(ex1);
        Assert.Null(ex2);
        Assert.Null(ex3);
    }

    //------------------------------------------------------------
    // Multiple subscribers
    //------------------------------------------------------------

    [Fact]
    public void SendMultipleSubscribersAllReceiveMessage()
    {
        var messenger = new Messenger();
        var count = 0;

        messenger.Received += (_, _) => count++;
        messenger.Received += (_, _) => count++;
        messenger.Received += (_, _) => count++;

        messenger.Send("Broadcast");

        Assert.Equal(3, count);
    }

    //------------------------------------------------------------
    // Singleton Default
    //------------------------------------------------------------

    [Fact]
    public void DefaultIsSingleton()
    {
        var a = Messenger.Default;
        var b = Messenger.Default;

        Assert.Same(a, b);
    }
}
