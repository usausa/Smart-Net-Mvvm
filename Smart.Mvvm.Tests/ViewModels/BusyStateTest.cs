namespace Smart.Mvvm.ViewModels;

public sealed class BusyStateTest
{
    //------------------------------------------------------------
    // Require / Release
    //------------------------------------------------------------

    [Fact]
    public void RequireTwiceIsBusyRemainsTrueAfterOneRelease()
    {
        var state = new BusyState();

        state.Require();
        state.Require();

        Assert.True(state.IsBusy);

        state.Release();

        Assert.True(state.IsBusy);
    }

    [Fact]
    public void RequireTwiceIsBusyFalseAfterTwoReleases()
    {
        var state = new BusyState();

        state.Require();
        state.Require();
        state.Release();
        state.Release();

        Assert.False(state.IsBusy);
    }

    //------------------------------------------------------------
    // PropertyChanged
    //------------------------------------------------------------

    [Fact]
    public void RequirePropertyChangedFiredOnceOnFirstRequire()
    {
        var state = new BusyState();
        var fireCount = 0;
        state.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBusyState.IsBusy))
            {
                fireCount++;
            }
        };

        state.Require();

        Assert.Equal(1, fireCount);
    }

    [Fact]
    public void RequirePropertyChangedNotFiredAgainOnSecondRequire()
    {
        var state = new BusyState();
        state.Require();

        var fireCount = 0;
        state.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBusyState.IsBusy))
            {
                fireCount++;
            }
        };

        state.Require();

        Assert.Equal(0, fireCount);
    }

    [Fact]
    public void ReleasePropertyChangedFiredOnceOnTransitionToFalse()
    {
        var state = new BusyState();
        state.Require();

        var fireCount = 0;
        state.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBusyState.IsBusy))
            {
                fireCount++;
            }
        };

        state.Release();

        Assert.Equal(1, fireCount);
    }

    //------------------------------------------------------------
    // Reset
    //------------------------------------------------------------

    [Fact]
    public void ResetWhenBusySetsIsBusyFalseAndFiresPropertyChanged()
    {
        var state = new BusyState();
        state.Require();
        state.Require();

        var fireCount = 0;
        state.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBusyState.IsBusy))
            {
                fireCount++;
            }
        };

        state.Reset();

        Assert.False(state.IsBusy);
        Assert.Equal(1, fireCount);
    }

    [Fact]
    public void ResetWhenNotBusyDoesNotFirePropertyChanged()
    {
        var state = new BusyState();

        var fireCount = 0;
        state.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBusyState.IsBusy))
            {
                fireCount++;
            }
        };

        state.Reset();

        Assert.False(state.IsBusy);
        Assert.Equal(0, fireCount);
    }

    //------------------------------------------------------------
    // Begin
    //------------------------------------------------------------

    [Fact]
    public void BeginInsideUsingScopeIsBusyTrue()
    {
        var state = new BusyState();

        using (state.Begin())
        {
            Assert.True(state.IsBusy);
        }
    }

    [Fact]
    public void BeginAfterUsingScopeIsBusyFalse()
    {
        var state = new BusyState();

        using (state.Begin())
        {
        }

        Assert.False(state.IsBusy);
    }

    [Fact]
    public void BeginNestedScopesIsBusyFalseAfterBothDisposed()
    {
        var state = new BusyState();

        var outer = state.Begin();
        var inner = state.Begin();

        Assert.True(state.IsBusy);

        inner.Dispose();
        Assert.True(state.IsBusy);

        outer.Dispose();
        Assert.False(state.IsBusy);
    }

    //------------------------------------------------------------
    // Discovered
    //------------------------------------------------------------

    [Fact]
    public void ReleaseBelowZeroIsBusyStaysFalse()
    {
        var state = new BusyState();

        state.Release();

        Assert.False(state.IsBusy);
    }

    [Fact]
    public void ReleaseBelowZeroThenRequireSetsIsBusyTrue()
    {
        var state = new BusyState();

        state.Release();
        state.Require();

        Assert.True(state.IsBusy);
    }

    [Fact]
    public void ReleaseWhenNotBusyDoesNotFirePropertyChanged()
    {
        var state = new BusyState();

        var fireCount = 0;
        state.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBusyState.IsBusy))
            {
                fireCount++;
            }
        };

        state.Release();

        Assert.Equal(0, fireCount);
    }
}
