using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Selenium.Linkedin
{
    class Program
    {
        private static IWebDriver m_driver;

        static void Main(string[] args)
        {
            Console.WriteLine("Selenium Iniciado");

            m_driver = new FirefoxDriver();
            m_driver.Url = "https://www.linkedin.com";
            m_driver.Manage().Window.Maximize();
            
            Login();

            AcceptInviations();

            //RequestSkills();
        }

        private static void Login()
        {
            IWebElement emailTextBox = m_driver.FindElement(By.XPath(".//*[@name='session_key']"));
            emailTextBox.SendKeys(User.Email);
            IWebElement passwordTextBox = m_driver.FindElement(By.XPath(".//*[@name='session_password']"));
            passwordTextBox.SendKeys(User.Password);

            IWebElement buttonSignin = m_driver.FindElement(By.XPath(".//*[@class='sign-in-form__submit-button']"));
            buttonSignin.Click();
        }

        private static void AcceptInviations()
        {
            
            m_driver.Navigate().GoToUrl("https://www.linkedin.com/mynetwork/invitation-manager/");

            Thread.Sleep(2000);

            IJavaScriptExecutor js = m_driver as IJavaScriptExecutor;

            //Get All invites
            ICollection<IWebElement> inviations = m_driver.FindElements(By.XPath(".//*[@class='invitation-card artdeco-list__item ember-view']"));


            foreach (var inviation in inviations)
            {

                //Check if the invitation message is customized
                if (ElementExistItem(inviation, ".invitation-card__custom-message-container")) continue;


                if (!ElementExistItem(inviation, ".invitation-card__title")) continue;

                //Get the name of the person who sent the invitation
                var name = inviation.FindElement(By.ClassName("invitation-card__title")).Text;
                Console.WriteLine($"Nome: {name}");
                
                //Open Chat
                inviation.FindElement(By.ClassName("message-anywhere-button")).Click();
                Thread.Sleep(1000);

                //Write Message in chat
                SendMessage(Messages.Presentation(name));

                //Accept Invite
                var buttons = inviation.FindElements(By.ClassName("invitation-card__action-btn"));
                buttons[1].Click();

                Thread.Sleep(2500);

                //Down Scroll
                js.ExecuteScript($"window.scrollBy(0,-80)");
            }
        }

        private static void RequestSkills()
        {
            ICollection<IWebElement> friends;
            m_driver.Navigate().GoToUrl("https://www.linkedin.com/mynetwork/invite-connect/connections/");
            Thread.Sleep(5000);

            string countConections = m_driver.FindElement(By.CssSelector(".mn-connections__header")).Text;
            Console.WriteLine($"Quantidade Contatos: {countConections}");

            IJavaScriptExecutor js = m_driver as IJavaScriptExecutor;
            int countScroll = 0, countScrollPixel = 2000, countFriends =0;

            do
            {
                friends = m_driver.FindElements(By.ClassName("mn-connection-card"));
                Console.WriteLine(friends.Count);
                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollBy(0,2000)");
                Thread.Sleep(1000);
                js.ExecuteScript("window.scrollBy(0,-50)");              

                if(countFriends != friends.Count){
                    countScroll = 0;
                    countFriends = friends.Count;
                }
                else{
                    countScroll += 1;
                }

                countScrollPixel += 2000;

                if (countScroll > 5)
                    break;

            } while (true);

            Thread.Sleep(1000);
            js.ExecuteScript($"window.scrollBy(0,{-1 * countScrollPixel})");

            foreach (var item in friends)
            {
                //Verifica se 
                if (!ElementExistItem(item, ".message-anywhere-button")) continue;

                //Pega nome usuário
                var name = item.FindElement(By.CssSelector(".mn-connection-card__name")).Text;
                Console.WriteLine($"Nome: {name}");

                //Abre chat usuário
                item.FindElement(By.CssSelector(".message-anywhere-button")).Click();
                Thread.Sleep(2000);

                //Preenche formúlário mensagem

                if (!ElementExistBody(m_driver, ".msg-s-event-listitem__body")) {
                    //SendMessage(Messages.Presentation(name));
                    m_driver.FindElement(By.ClassName("js-msg-close")).Click();
                    continue;
                }


                IWebElement chatMessageAbility = m_driver.FindElement(By.CssSelector(".msg-s-event-listitem__body"));

                if (!chatMessageAbility.Text.Contains("5 principais habilidades em seu perfil"))
                {
                    //SendMessage(Messages.Skills(name));
                }
                //Fecha chat
                m_driver.FindElement(By.ClassName("js-msg-close")).Click();

                js.ExecuteScript("window.scrollBy(0,80)");
            }
        }
        
    
        private static bool ElementExistItem(IWebElement item, string name)
        {

            try
            {
                IWebElement exist =  item.FindElement(By.CssSelector(name));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ElementExistBody(IWebDriver driver, string name)
        {

            try
            {
                IWebElement exist = driver.FindElement(By.CssSelector(name));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void SendMessage(string message)
        {
            IWebElement chat = m_driver.FindElement(By.ClassName("msg-form__contenteditable"));
            chat.SendKeys(message);

            Thread.Sleep(2000);

            //Send Message
            m_driver.FindElement(By.ClassName("msg-form__send-button")).Click();
            Thread.Sleep(5000);

            //Close chat
            var close = m_driver.FindElement(By.XPath("//button[contains(@data-control-name, 'overlay.close_conversation_window')]"));
            close.Click();
            
            Thread.Sleep(2000);
        }

    }
}
