﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Gi7.Client;
using Gi7.Client.Model;
using Gi7.Client.Model.Event;
using Gi7.Client.Model.Extra;
using Gi7.Client.Request;
using Gi7.Client.Request.Event;
using Gi7.Client.Request.Repository;
using Gi7.Service;
using Gi7.Service.Navigation;
using Gi7.Utils;
using Microsoft.Phone.Controls;
using UserRequest = Gi7.Client.Request.User;

namespace Gi7.Views
{
    public class HomeViewModel : ViewModelBase
    {
        private readonly GithubService _githubService;
        private ListReceived _eventsRequest;
        private UserRequest.ListFollowers _followersRequest;
        private UserRequest.ListFollowings _followingsRequest;
        private ObservableCollection<Repository> _ownedRepos;
        private ObservableCollection<Repository> _watchedRepos;
        private ObservableCollection<FeaturedRepo> _featuredRepos;
        private ObservableCollection<SearchResult> _searchResults;
        private User _user;
        private String _search;
        private bool _isLoggedIn;

        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand AboutCommand { get; private set; }
        public RelayCommand<Event> EventSelectedCommand { get; private set; }
        public RelayCommand<User> UserSelectedCommand { get; private set; }
        public RelayCommand<Repository> RepoSelectedCommand { get; private set; }
        public RelayCommand<FeaturedRepo> FeaturedRepoSelectedCommand { get; private set; }
        public RelayCommand<SelectionChangedEventArgs> PanoramaChangedCommand { get; private set; }
        public RelayCommand<SearchResult> ResultSelectedCommand { get; private set; }

        public HomeViewModel(GithubService githubService, INavigationService navigationService)
        {
            _githubService = githubService;

            // commands
            FeaturedRepoSelectedCommand = new RelayCommand<FeaturedRepo>(r =>
            {
                if (r != null)
                    navigationService.NavigateTo(String.Format(ViewModelLocator.RepositoryUrl, r.User, r.Repo));
            });
            RepoSelectedCommand = new RelayCommand<Repository>(r =>
            {
                if (r != null)
                    navigationService.NavigateTo(String.Format(ViewModelLocator.RepositoryUrl, r.Owner.Login, r.Name));
            });
            EventSelectedCommand = new RelayCommand<Event>(e =>
            {
                if (e != null)
                    navigationService.NavigateTo(new EventManager().GetDestination(e));
            });
            UserSelectedCommand = new RelayCommand<User>(user =>
            {
                if (user != null)
                    navigationService.NavigateTo(string.Format(ViewModelLocator.UserUrl, user.Login));
            });
            ResultSelectedCommand = new RelayCommand<SearchResult>(r =>
            {
                if (r.Type == "user")
                {
                    navigationService.NavigateTo(string.Format(ViewModelLocator.UserUrl, r.Name));
                }
                else // repo
                {
                    var repoData = r.Name.Split('/');
                    navigationService.NavigateTo(string.Format(ViewModelLocator.RepositoryUrl, repoData[0].Trim(), repoData[1].Trim()));
                }
            });
            PanoramaChangedCommand = new RelayCommand<SelectionChangedEventArgs>(args => { _loadPanel((args.AddedItems[0] as PanoramaItem).Header as String); });
            AboutCommand = new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.AboutUrl));
            LogoutCommand = new RelayCommand(() => githubService.Logout(), () => IsLoggedIn);

            // init
            if (_githubService.IsAuthenticated)
                _login();
            else
                _logout();

            // listenning to the github service
            githubService.IsAuthenticatedChanged += (s, e) =>
            {
                if (e.IsAuthenticated)
                    _login();
                else
                    _logout();
            };

            // listenning to the search box
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Search")
                {
                    SearchResults = githubService.Load(new Search(Search), r => SearchResults = r);
                }
            };
        }

        public bool IsLoggedIn
        {
            get { return _isLoggedIn; }
            set
            {
                if (_isLoggedIn != value)
                {
                    _isLoggedIn = value;
                    RaisePropertyChanged("IsLoggedIn");
                    RaisePropertyChanged("IsLoggedOut");
                    LogoutCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoggedOut
        {
            get { return !IsLoggedIn; }
        }

        public User User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    RaisePropertyChanged("User");
                }
            }
        }

        public ListReceived EventsRequest
        {
            get { return _eventsRequest; }
            set
            {
                if (_eventsRequest != value)
                {
                    _eventsRequest = value;
                    RaisePropertyChanged("EventsRequest");
                }
            }
        }

        public ObservableCollection<Repository> OwnedRepos
        {
            get { return _ownedRepos; }
            set
            {
                if (_ownedRepos != value)
                {
                    _ownedRepos = value;
                    RaisePropertyChanged("OwnedRepos");
                }
            }
        }

        public ObservableCollection<Repository> WatchedRepos
        {
            get { return _watchedRepos; }
            set
            {
                if (_watchedRepos != value)
                {
                    _watchedRepos = value;
                    RaisePropertyChanged("WatchedRepos");
                }
            }
        }

        public UserRequest.ListFollowings FollowingsRequest
        {
            get { return _followingsRequest; }
            set
            {
                if (_followingsRequest != value)
                {
                    _followingsRequest = value;
                    RaisePropertyChanged("FollowingsRequest");
                }
            }
        }

        public UserRequest.ListFollowers FollowersRequest
        {
            get { return _followersRequest; }
            set
            {
                if (_followersRequest != value)
                {
                    _followersRequest = value;
                    RaisePropertyChanged("FollowersRequest");
                }
            }
        }

        public ObservableCollection<FeaturedRepo> FeaturedRepos
        {
            get { return _featuredRepos; }
            set
            {
                if (_featuredRepos != value)
                {
                    _featuredRepos = value;
                    RaisePropertyChanged("FeaturedRepos");
                }
            }
        }

        public ObservableCollection<SearchResult> SearchResults
        {
            get { return _searchResults; }
            set
            {
                if (_searchResults != value)
                {
                    _searchResults = value;
                    RaisePropertyChanged("SearchResults");
                }
            }
        }

        public String Search
        {
            get { return _search; }
            set
            {
                if (_search != value)
                {
                    _search = value;
                    RaisePropertyChanged("Search");
                }
            }
        }

        private void _loadPanel(string header)
        {
            switch (header)
            {
                case "News Feed":
                    if (EventsRequest == null)
                    {
                        EventsRequest = new ListReceived(_githubService.Username);
                    }
                    break;
                case "Repos":
                    if (OwnedRepos == null)
                    {
                        OwnedRepos = _githubService.Load(new List());
                        _githubService.Load(new ListWatched(_githubService.Username), result =>
                        {
                            WatchedRepos = new ObservableCollection<Repository>(result.Where(repo => !repo.Owner.Login.Equals(_githubService.Username, StringComparison.InvariantCultureIgnoreCase)));
                        });
                    }
                    break;
                case "Follower":
                    if (FollowersRequest == null)
                        FollowersRequest = new UserRequest.ListFollowers(_githubService.Username);
                    break;
                case "Following":
                    if (FollowingsRequest == null)
                        FollowingsRequest = new UserRequest.ListFollowings(_githubService.Username);
                    break;
                case "Profile":
                    if (User == null)
                        User = _githubService.Load(new UserRequest.Get(_githubService.Username), u => User = u);
                    break;
                case "Explore":
                    if (FeaturedRepos == null)
                        FeaturedRepos = _githubService.Load(new FeaturedRepoRequest());
                    break;
                default:
                    break;
            }
        }

        private void _login()
        {
            IsLoggedIn = true;

            _loadPanel("News Feed");
        }

        private void _logout()
        {
            IsLoggedIn = false;
            User = null;
            EventsRequest = null;
            OwnedRepos = null;
            WatchedRepos = null;
            FollowersRequest = null;
            FollowingsRequest = null;
        }
    }
}