﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using DevelopmentInProgress.DipState;
using DevelopmentInProgress.ExampleModule.Model;
using DevelopmentInProgress.ExampleModule.Service;
using DevelopmentInProgress.Origin.Context;
using DevelopmentInProgress.Origin.ViewModel;
using DevelopmentInProgress.WPFControls.Messaging;

namespace DevelopmentInProgress.ExampleModule.ViewModel
{
    public class CustomerRemediationViewModel : DocumentViewModel
    {
        private readonly RemediationService remediationService;

        public CustomerRemediationViewModel(ViewModelContext viewModelContext, RemediationService remediationService)
            : base(viewModelContext)
        {
            this.remediationService = remediationService;
            CompleteCommand = new ViewModelCommand(Complete);
            FailCommand = new ViewModelCommand(Fail);

            Products = new List<string>() {"Cap", "Collar", "Structured Collar"};
        }

        public ICommand CompleteCommand { get; set; }
        public ICommand FailCommand { get; set; }
        public List<Customer> Customers { get; set; }
        public Customer CurrentCustomer { get; set; }
        public List<string> Products { get; set; }

        protected async override void OnPublished(object data)
        {
            try
            {
                IsBusy = true;
                ClearMessages();
                Customers = await remediationService.GetCustomersAsync();
                ResetStatus();
                OnPropertyChanged("");
            }
            catch (Exception ex)
            {
                ShowMessage(new Message() { MessageType = MessageType.Error, Text = ex.Message }, true);
            }
        }

        private async void Complete(object param)
        {
            ClearMessages();
            IsBusy = true;

            var state = param as EntityBase;
            state.InProgress = true;

            try
            {
                var currentState = await remediationService.CompleteStateAsync(state);
            }
            catch (StateException ex)
            {
                ShowMessage(new Message() {MessageType = MessageType.Warn, Text = ex.Message}, true);
            }
            catch (Exception ex)
            {
                ShowMessage(new Message() { MessageType = MessageType.Error, Text = ex.Message });
            }
            finally
            {
                IsBusy = false;
                state.InProgress = false;
                CurrentCustomer.Refresh();
            }
        }
        
        private async void Fail(object param)
        {
            ClearMessages();
            IsBusy = true;

            var state = param as EntityBase;
            state.InProgress = true;

            try
            {
                var currentState = await remediationService.FailToCollateData(state);
            }
            catch (StateException ex)
            {
                ShowMessage(new Message() {MessageType = MessageType.Warn, Text = ex.Message}, true);
            }
            catch (Exception ex)
            {
                ShowMessage(new Message() {MessageType = MessageType.Error, Text = ex.Message});
            }
            finally
            {
                IsBusy = false;
                state.InProgress = false;
                CurrentCustomer.Refresh();
            }
        }
    }
}
