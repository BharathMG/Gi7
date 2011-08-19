﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Gi7.Model;
using Gi7.Service;
using Gi7.Service.Navigation;
using Microsoft.Phone.Controls;

namespace Gi7.Views
{
    public class RepositoryViewModel : ViewModelBase
    {
        private Repository _repository;
        public Repository Repository
        {
            get { return _repository; }
            set
            {
                if (_repository != value)
                {
                    _repository = value;
                    RaisePropertyChanged("Repository");
                }
            }
        }

        private ObservableCollection<Push> _commits;
        public ObservableCollection<Push> Commits
        {
            get { return _commits; }
            set
            {
                if (_commits != value)
                {
                    _commits = value;
                    RaisePropertyChanged("Commits");
                }
            }
        }

        private ObservableCollection<PullRequest> _pullRequests;
        public ObservableCollection<PullRequest> PullRequests
        {
            get { return _pullRequests; }
            set
            {
                if (_pullRequests != value)
                {
                    _pullRequests = value;
                    RaisePropertyChanged("PullRequests");
                }
            }
        }

        private ObservableCollection<Issue> _issues;
        public ObservableCollection<Issue> Issues
        {
            get { return _issues; }
            set
            {
                if (_issues != value)
                {
                    _issues = value;
                    RaisePropertyChanged("Issues");
                }
            }
        }

        public RelayCommand OwnerCommand { get; private set; }
        public RelayCommand<SelectionChangedEventArgs> PivotChangedCommand { get; private set; }
        public RelayCommand<Push> CommitSelectedCommand { get; private set; }
        public RelayCommand<PullRequest> PullRequestSelectedCommand { get; private set; }
        public RelayCommand<Issue> IssueSelectedCommand { get; private set; }

        public RepositoryViewModel(GithubService githubService, INavigationService navigationService, String user, String repo)
        {
            Repository = githubService.GetRepository(user, repo, r => Repository = r);

            PullRequests = githubService.GetPullRequests(user, repo);

            Issues = githubService.GetIssues(user, repo);

            OwnerCommand = new RelayCommand(() => navigationService.NavigateTo(String.Format(ViewModelLocator.UserUrl, Repository.Owner.Login)));
            PivotChangedCommand = new RelayCommand<SelectionChangedEventArgs>(args =>
            {
                var header = (args.AddedItems[0] as PivotItem).Header as String;
                switch (header)
                {
                    case "Commits":
                        if(Commits == null)
                            Commits = githubService.GetCommits(user, repo);
                        break;
                    default:
                        break;
                }
            });
            CommitSelectedCommand = new RelayCommand<Push>(push =>
            {
                if (push != null)
                {
                    navigationService.NavigateTo(String.Format(ViewModelLocator.CommitUrl, Repository.Owner.Login, Repository.Name, push.Sha));
                }
            });
            PullRequestSelectedCommand = new RelayCommand<PullRequest>(pullRequest =>
            {
                if (pullRequest != null)
                {
                    navigationService.NavigateTo(String.Format(ViewModelLocator.PullRequestUrl, Repository.Owner.Login, Repository.Name, pullRequest.Number));
                }
            });
            IssueSelectedCommand = new RelayCommand<Issue>(issue =>
            {
                if (issue != null)
                {
                    var destination = issue.PullRequest.HtmlUrl == null ? ViewModelLocator.IssueUrl : ViewModelLocator.PullRequestUrl;
                    navigationService.NavigateTo(String.Format(destination, Repository.Owner.Login, Repository.Name, issue.Number));
                }
            });
        }
    }
}
