using SukiUI.Enums;
using System;

namespace SukiUI.Models;

public readonly record struct SukiToastModel(string Title, object Content, SukiToastType Type, TimeSpan Lifetime, Action? OnClicked)
{
    public string Title { get; } = Title;
    public object Content { get; } = Content;
    public SukiToastType Type { get; } = Type;
    public TimeSpan Lifetime { get; } = Lifetime;
    public Action? OnClicked { get; } = OnClicked;
}