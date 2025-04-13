using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Modules.Network;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace first_test_lection;

public class Tests
{
    private IWebDriver driver;
    private WebDriverWait wait;

    [SetUp]
    public void Setup()
    {
        var options = new ChromeOptions();
        options.AddArgument("--start-maximized");

        driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        Login("Почта", "Пароль");
    }

    [TearDown]
    public void Teardown() 
    {
    driver.Quit();
    driver.Dispose();
    }

    private void Login(string username, string password)
    {
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/");
        driver.FindElement(By.Id("Username")).SendKeys(username);
        driver.FindElement(By.Id("Password")).SendKeys(password);
        driver.FindElement(By.Name("button")).Click();

        wait.Until(ExpectedConditions.UrlToBe("https://staff-testing.testkontur.ru/news"));
    }

    private void OpenProfileMenu()
    {
        var profileMenu = driver.FindElement(By.CssSelector("[data-tid='ProfileMenu']"));
        profileMenu.Click();
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='PopupContentInner']")));
    }

     private void OpenSettings()
    {
        OpenProfileMenu();
        var settings = driver.FindElement(By.CssSelector("[data-tid='Settings']"));
        settings.Click();
    }

    [Test]
    public void LogoutTest()
    {
        OpenProfileMenu();

        var logout = driver.FindElement(By.CssSelector("[data-tid='Logout']"));
        logout.Click();
        
        var logoutHeader = wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("body-wrapper")));;    
        Assert.That(logoutHeader.Text, Is.EqualTo("Вы вышли из учетной записи\r\nВернуться в Кадровый Портал"), "Текст после выхода не совпадает с ожидаемым");
    }

    [Test]
    public void SearchTest()
    {
        var search = driver.FindElement(By.CssSelector("[data-tid='SearchBar']"));
        search.Click();

        var searchInput = driver.FindElement(By.CssSelector("[placeholder='Поиск сотрудника, подразделения, сообщества, мероприятия']"));
        searchInput.SendKeys("Беленцов Александр");

        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='ScrollContainer__inner']")));

        var searchResult = driver.FindElement(By.CssSelector("[data-tid='ComboBoxMenu__item']"));
        searchResult.Click();

        var employeeName = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='EmployeeName']")));
        Assert.Multiple(() =>
        {
            Assert.That(employeeName.Text, Is.EqualTo("Беленцов Александр"), "Открылась страница не того сотрудника");
            Assert.That(driver.Url, Is.EqualTo("https://staff-testing.testkontur.ru/profile/11e4e2d0-581d-4b19-8b6a-b281beb0dc3c"), "Не открылась страница сотрудника");
        });
    }

    [Test]
    public void NewYearThemeTest()
    {
        OpenSettings();

        var switchElement = driver.FindElement(By.CssSelector("[class='react-ui-1jxed06']")); //Не нашел лучшей подвязки
        switchElement.Click();

        var saveButton = driver.FindElement(By.XPath("//button[.//span[text()='Сохранить']]"));
        saveButton.Click();
        
        var garland = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("[class='sc-hOPeYd eXirsP']")));; //Не нашел лучшей подвязки
        Assert.That(garland.Displayed, Is.True, "Новогодняя тема не активировалась: гирлянда не отображается");

    }

    [Test]
    public void CreateCommunityTest()
    {
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities");
        wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("[data-tid='PageHeader']")));

        var createCommunityButton = driver.FindElement(By.CssSelector("[data-tid='PageHeader']")).FindElement(By.TagName("button"));
        createCommunityButton.Click();

        var communityNameField = driver.FindElement(By.CssSelector("[data-tid='Name']"));
        communityNameField.SendKeys("Я создано автотестом :)");

        var communityDescriptionField = driver.FindElement(By.CssSelector("[data-tid='Message']"));
        communityDescriptionField.SendKeys("P.s. Скоро я научусь завоевывать мир!");

        var createButton = driver.FindElement(By.CssSelector("[data-tid='CreateButton']"));
        createButton.Click();

        var communityName = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(" [data-tid='SettingsTabWrapper'] [placeholder='Название сообщества']")));
        Assert.That(communityName.Text, Is.EqualTo("Я создано автотестом :)"), "Страница редактирования нового сообщества не загрузилась");
    }

    [Test]
    public void DiscussionCommentTest()
    {
        driver.Navigate().GoToUrl("https://staff-testing.testkontur.ru/communities/e8ce0b22-dd03-4669-b21d-53c986425976?tab=discussions&id=6e670e6a-bdec-4265-b0d8-e1ead2e95b48");
        wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[placeholder='Комментировать...']")));
        var commentArea = driver.FindElement(By.CssSelector("[placeholder='Комментировать...']"));
        commentArea.Click();

        var commentInput = driver.FindElement(By.CssSelector("[data-tid='CommentInput']"));
        commentInput.SendKeys("Автотест следит за нами");

        var sendButton = driver.FindElement(By.CssSelector("[data-tid='SendComment']"));
        sendButton.Click();

        var sendTimeComment = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-tid='TextComment' and contains(text(), 'Автотест следит за нами')]")));
        Assert.That(sendTimeComment.Text, Is.EqualTo("Автотест следит за нами"), "Комментарий имеет другое содержание");
    }

    [Test]
    public void AuthorizationTest()
    {
        var titlePageElement = driver.FindElement(By.CssSelector("[data-tid='Title']"));

        Assert.Multiple(() =>
        {
            Assert.That(titlePageElement.Text, Is.EqualTo("Новости"), "Страница Новости не загрузилась");
            Assert.That(driver.Title, Is.EqualTo("Новости"), "Заголовок страницы неккоректный");
        });
    }

    [Test]
    public void NavigatingToCommunitiesTest()
    {
        wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-tid='PageMainMenu'] [data-tid='Community']")));

        var communities = driver.FindElement(By.CssSelector("[data-tid='PageMainMenu'] [data-tid='Community']"));
        communities.Click();
        
        wait.Until(ExpectedConditions.UrlToBe("https://staff-testing.testkontur.ru/communities"));
        var titlePageElement = driver.FindElement(By.CssSelector("[data-tid='Title']"));

        Assert.That(titlePageElement.Text, Is.EqualTo("Сообщества"), "На странице Сообщества заголовок некорректен");
    }
}
