﻿using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using GitHub.Factories;
using GitHub.InlineReviews.Views;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.InlineReviews.Commands
{
    /// <summary>
    /// Shows the pull request comments view for a specified pull request.
    /// </summary>
    [ExportCommand(typeof(InlineReviewsPackage))]
    class ShowPullRequestCommentsCommand : Command<IPullRequestModel>
    {
        public static readonly Guid CommandSet = GlobalCommands.CommandSetGuid;
        public const int CommandId = GlobalCommands.ShowPullRequestCommentsId;

        readonly IApiClientFactory apiClientFactory;
        readonly IPullRequestSessionManager sessionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowPullRequestCommentsCommand"/> class.
        /// </summary>
        /// <param name="apiClientFactory">The API client factory.</param>
        /// <param name="sessionManager">The pull request session manager.</param>
        [ImportingConstructor]
        public ShowPullRequestCommentsCommand(
            IApiClientFactory apiClientFactory,
            IPullRequestSessionManager sessionManager)
            : base(CommandSet, CommandId)
        {
            this.apiClientFactory = apiClientFactory;
            this.sessionManager = sessionManager;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="pullRequest">The pull request.</param>
        /// <returns>A task that tracks the execution of the command.</returns>
        protected override async Task Execute(IPullRequestModel pullRequest)
        {
            if (pullRequest == null) return;

            var package = (Microsoft.VisualStudio.Shell.Package)Package;
            var window = (PullRequestCommentsPane)package.FindToolWindow(
                typeof(PullRequestCommentsPane), pullRequest.Number, true);

            if (window?.Frame == null)
            {
                throw new NotSupportedException("Cannot create Pull Request Comments tool window");
            }

            var session = await sessionManager.GetSession(pullRequest);
            var address = HostAddress.Create(session.Repository.CloneUrl);
            var apiClient = apiClientFactory.Create(address);
            await window.Initialize(session, apiClient);

            var windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
