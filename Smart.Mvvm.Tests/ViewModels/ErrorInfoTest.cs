namespace Smart.Mvvm.ViewModels;

public class ErrorInfoTest
{
    //------------------------------------------------------------
    // AddError
    //------------------------------------------------------------

    [Fact]
    public void AddErrorSetsHasErrorTrue()
    {
        using var info = new ErrorInfo();

        info.AddError("Name", "required");

        Assert.True(info.HasError);
    }

    [Fact]
    public void AddErrorIndexerReturnsFirstMessage()
    {
        using var info = new ErrorInfo();

        info.AddError("Name", "first");
        info.AddError("Name", "second");

        Assert.Equal("first", info["Name"]);
    }

    [Fact]
    public void AddErrorGetErrorsReturnsAllMessages()
    {
        using var info = new ErrorInfo();

        info.AddError("Name", "msg1");
        info.AddError("Name", "msg2");

        var errors = info.GetErrors("Name");
        Assert.Equal(2, errors.Count);
        Assert.Contains("msg1", errors);
        Assert.Contains("msg2", errors);
    }

    //------------------------------------------------------------
    // AddErrors
    //------------------------------------------------------------

    [Fact]
    public void AddErrorsAddsMultipleMessages()
    {
        using var info = new ErrorInfo();

        info.AddErrors("Age", ["must be positive", "must be less than 200"]);

        Assert.True(info.HasError);
        Assert.Equal(2, info.GetErrors("Age").Count);
    }

    [Fact]
    public void AddErrorsEmptySequenceDoesNotSetHasError()
    {
        using var info = new ErrorInfo();

        info.AddErrors("Key", []);

        Assert.False(info.HasError);
    }

    //------------------------------------------------------------
    // UpdateError
    //------------------------------------------------------------

    [Fact]
    public void UpdateErrorReplacesExistingMessages()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "old1");
        info.AddError("Name", "old2");

        info.UpdateError("Name", "new");

        Assert.Single(info.GetErrors("Name"));
        Assert.Equal("new", info["Name"]);
    }

    //------------------------------------------------------------
    // UpdateErrors
    //------------------------------------------------------------

    [Fact]
    public void UpdateErrorsReplacesWithMultipleMessages()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "old");

        info.UpdateErrors("Name", ["a", "b"]);

        var errors = info.GetErrors("Name");
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void UpdateErrorsEmptySequenceClearsKeyErrors()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");

        info.UpdateErrors("Name", []);

        Assert.False(info.Contains("Name"));
    }

    [Fact]
    public void UpdateErrorsEmptySequenceOtherKeyErrorsMaintainHasError()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        info.UpdateErrors("Name", []);

        Assert.True(info.HasError);
        Assert.True(info.Contains("Age"));
    }

    //------------------------------------------------------------
    // ClearErrors
    //------------------------------------------------------------

    [Fact]
    public void ClearErrorsRemovesSpecificKey()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");

        info.ClearErrors("Name");

        Assert.False(info.Contains("Name"));
        Assert.Null(info["Name"]);
    }

    [Fact]
    public void ClearErrorsOtherKeyErrorsHasErrorRemainsTrue()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        info.ClearErrors("Name");

        Assert.True(info.HasError);
        Assert.True(info.Contains("Age"));
    }

    [Fact]
    public void ClearErrorsLastKeySetsHasErrorFalse()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");

        info.ClearErrors("Name");

        Assert.False(info.HasError);
    }

    [Fact]
    public void ClearErrorsNonExistentKeyDoesNotThrow()
    {
        using var info = new ErrorInfo();

        // ReSharper disable once AccessToDisposedClosure
        var ex = Record.Exception(() => info.ClearErrors("NonExistent"));

        Assert.Null(ex);
    }

    //------------------------------------------------------------
    // ClearAllErrors
    //------------------------------------------------------------

    [Fact]
    public void ClearAllErrorsSetsHasErrorFalse()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        info.ClearAllErrors();

        Assert.False(info.HasError);
        Assert.False(info.Contains("Name"));
        Assert.False(info.Contains("Age"));
    }

    [Fact]
    public void ClearAllErrorsWhenNoErrorsDoesNotThrow()
    {
        using var info = new ErrorInfo();

        var ex = Record.Exception(info.ClearAllErrors);

        Assert.Null(ex);
    }

    //------------------------------------------------------------
    // Contains
    //------------------------------------------------------------

    [Fact]
    public void ContainsReturnsTrueWhenKeyHasErrors()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");

        Assert.True(info.Contains("Name"));
    }

    [Fact]
    public void ContainsReturnsFalseWhenKeyHasNoErrors()
    {
        using var info = new ErrorInfo();

        Assert.False(info.Contains("Missing"));
    }

    //------------------------------------------------------------
    // GetKeys
    //------------------------------------------------------------

    [Fact]
    public void GetKeysReturnsOnlyKeysWithErrors()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        var keys = info.GetKeys().ToList();

        Assert.Contains("Name", keys);
        Assert.Contains("Age", keys);
        Assert.Equal(2, keys.Count);
    }

    [Fact]
    public void GetKeysWhenNoErrorsReturnsEmptySequence()
    {
        using var info = new ErrorInfo();

        Assert.Empty(info.GetKeys());
    }

    //------------------------------------------------------------
    // GetErrors
    //------------------------------------------------------------

    [Fact]
    public void GetErrorsNonExistentKeyReturnsEmpty()
    {
        using var info = new ErrorInfo();

        var errors = info.GetErrors("Missing");

        Assert.Empty(errors);
    }

    //------------------------------------------------------------
    // GetAllErrors
    //------------------------------------------------------------

    [Fact]
    public void GetAllErrorsReturnsAllMessagesAcrossKeys()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");
        info.AddError("Age", "err3");

        var all = info.GetAllErrors().ToList();

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void GetAllErrorsWhenNoErrorsReturnsEmpty()
    {
        using var info = new ErrorInfo();

        Assert.Empty(info.GetAllErrors());
    }

    //------------------------------------------------------------
    // Indexer
    //------------------------------------------------------------

    [Fact]
    public void IndexerNoErrorForKeyReturnsNull()
    {
        using var info = new ErrorInfo();

        Assert.Null(info["Unknown"]);
    }

    //------------------------------------------------------------
    // HasError PropertyChanged
    //------------------------------------------------------------

    [Fact]
    public void AddErrorHasErrorPropertyChangedFiredOnceForFirstError()
    {
        using var info = new ErrorInfo();
        var fireCount = 0;
        info.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ErrorInfo.HasError))
            {
                fireCount++;
            }
        };

        info.AddError("Name", "err");

        Assert.Equal(1, fireCount);
    }

    [Fact]
    public void AddErrorHasErrorPropertyChangedNotFiredForSubsequentErrors()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "first");

        var fireCount = 0;
        info.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ErrorInfo.HasError))
            {
                fireCount++;
            }
        };

        info.AddError("Name", "second");

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public void ClearAllErrorsHasErrorPropertyChangedFiredOnTransitionToFalse()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");

        var fireCount = 0;
        info.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ErrorInfo.HasError))
            {
                fireCount++;
            }
        };

        info.ClearAllErrors();

        Assert.Equal(1, fireCount);
    }

    //------------------------------------------------------------
    // Item[] PropertyChanged
    //------------------------------------------------------------

    [Fact]
    public void AddErrorItemsPropertyChangedFired()
    {
        using var info = new ErrorInfo();
        var fired = false;
        info.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "Item[]")
            {
                fired = true;
            }
        };

        info.AddError("Name", "err");

        Assert.True(fired);
    }

    [Fact]
    public void ClearAllErrorsItemsPropertyChangedFired()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");

        var fired = false;
        info.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "Item[]")
            {
                fired = true;
            }
        };

        info.ClearAllErrors();

        Assert.True(fired);
    }

    //------------------------------------------------------------
    // Key lifecycle
    //------------------------------------------------------------

    [Fact]
    public void ClearErrorsRemovesKeyFromGetKeys()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        info.ClearErrors("Name");

        Assert.Equal(["Age"], info.GetKeys());
    }

    [Fact]
    public void ClearAllErrorsRemovesAllKeysFromGetKeys()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        info.ClearAllErrors();

        Assert.Empty(info.GetKeys());
    }

    [Fact]
    public void UpdateErrorsEmptySequenceRemovesKeyFromGetKeys()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err1");
        info.AddError("Age", "err2");

        info.UpdateErrors("Name", []);

        Assert.Equal(["Age"], info.GetKeys());
    }

    [Fact]
    public void AddErrorAfterClearAllErrorsSetsHasError()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");
        info.ClearAllErrors();

        info.AddError("Name", "again");

        Assert.True(info.HasError);
        Assert.Equal("again", info["Name"]);
        Assert.Single(info.GetErrors("Name"));
    }

    [Fact]
    public void AddErrorAfterClearErrorsReusesKey()
    {
        using var info = new ErrorInfo();
        info.AddError("Name", "err");
        info.ClearErrors("Name");

        info.AddError("Name", "again");

        Assert.Equal(["again"], info.GetErrors("Name"));
    }
}
