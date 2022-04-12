using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VChesnokovBotTeamsApp1.Bots
{
    public class TeamsBot : ActivityHandler
    {
        private readonly string _welcomeAdaptiveCardTemplate = Path.Combine(".", "Resources", "WelcomeCardTemplate.json");
        private readonly string _learnAdaptiveCardTemplate = Path.Combine(".", "Resources", "LearnCardTemplate.json");
        private static readonly LikeCountObj likeCountObj = new LikeCountObj();

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
            AdaptiveCards.AdaptiveCard card = new AdaptiveCards.AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

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
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(CreateAdaptiveCardActivity(_welcomeAdaptiveCardTemplate, null)), cancellationToken);
                }
            }
        }

        protected override async Task<AdaptiveCardInvokeResponse> OnAdaptiveCardInvokeAsync(ITurnContext<IInvokeActivity> turnContext, AdaptiveCardInvokeValue invokeValue, CancellationToken cancellationToken)
        {
            if (invokeValue.Action.Verb == "userlike")
            {
                likeCountObj.Add();
                var adaptiveCard = CreateAdaptiveCardActivity(_learnAdaptiveCardTemplate, likeCountObj);
                Activity updateActivity = new Activity();
                updateActivity.Type = "message";
                updateActivity.Id = turnContext.Activity.ReplyToId;
                updateActivity.Attachments = new List<Attachment> { adaptiveCard };
                await turnContext.UpdateActivityAsync(updateActivity);
            }
            var response = new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = null,
                Value = null
            };
            return response;
        }

        private Attachment CreateAdaptiveCardActivity(string filePath, object dataObj)
        {
            var cardJSON = File.ReadAllText(filePath);
            if (dataObj != null)
            {
                AdaptiveCardTemplate template = new AdaptiveCardTemplate(cardJSON);
                cardJSON = template.Expand(dataObj);
            }
            var adaptiveCardAttachment = new Attachment
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJSON),
            };
            return adaptiveCardAttachment;
        }
    }

    internal class LikeCountObj
    {
        public int likeCount;

        public LikeCountObj()
        {
            likeCount = 0;
        }

        public void Add()
        {
            likeCount++;
        }
    }
}