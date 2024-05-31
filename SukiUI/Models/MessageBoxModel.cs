using System;
using SukiUI.Enums;

namespace SukiUI.Models
{
    public readonly record struct MessageBoxModel(string Title, object Content, NotificationType Type = NotificationType.Info, string? ActionButtonContent = null,Action? ActionButton= null)
    {
        public string Title { get; } = Title;
        public object Content { get; } = Content;
        public NotificationType Type { get; } = Type;
        
  

        public string? ActionButtonContent { get; } = ActionButtonContent;
        public Action? OnActionButtonClicked { get; } = ActionButton;
    }
}