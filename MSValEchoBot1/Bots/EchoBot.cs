// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MSValEchoBot1.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // 12.04.2022: В созданном проекте делаем изменения.
            // Когда в тексте пользователя содержится слово «время», выводить карточку Adaptivecards c датой и временем.
            // В остальных случаях Echo Bot будет возвращать введенный текст.
            string userInputText = turnContext.Activity.Text;

            if (!userInputText.ToLower().Contains("время")) // в любом регистре: "Время", "ВрЕмЯ" - тоже пройдёт.
            {
                // В остальных случаях Echo Bot будет возвращать введенный текст.
                string replyText = $"Echo: {userInputText}";

                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
                return;
            }

            // выводить карточку Adaptivecards c датой и временем.
            AdaptiveCard card = new AdaptiveCards.AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Введите дату",
                Size = AdaptiveTextSize.Medium
            });

            AdaptiveDateInput input = new AdaptiveDateInput()
            {
                Id = "Date",
                Placeholder = "Введите дату"
            };

            card.Body.Add(input);

            card.Actions.Add(new AdaptiveSubmitAction()
            {
                Title = "Сохранить"
            });

            Attachment cardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            // отправить медиа контент - карточку типа AdaptiveCard
            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
