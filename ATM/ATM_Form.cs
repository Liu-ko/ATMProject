using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Media;

namespace ATM
{
    public partial class ATM_Form : Form
    {
        // stores the number of state:
        // 0 - program is waiting for cart to be inserted  - Dom
        // 1 - program is waiting for PIN to be entered - Tania
        // 2 - user should choose operation to be performed - Dom
        // 3 - user should choose amount of money to be taken - Tania
        // 4 - take money, thanks for using us! three, two, one... Next please! - Dom
        // 5 - see the balance - Tania
        // 6 - screen to put money on your card
        //additional other states can be added
        private int state = 0;

        //local referance to the array of accounts
        private Account[] ac;
        private int pinTry;
        private SoundPlayer sound; // sound will be played if any button is pressed

        //this is a referance to the account that is being used
        private Account activeAccount = null; // acount that is user by user
        private bool datarace; // stores the ATM type. true - with datarace, false - ATM that prevents datarace

        #region initialization of the form

        // constructor for ATM_Form object
        // it receives array of accounts and boolean value that stores how ATM shoud run (true - with datarace, false - without it)
        public ATM_Form(Account[] ac, bool race)
        {
            InitializeComponent();
            this.ac = ac;
            this.datarace = race;
            sound = new SoundPlayer(Properties.Resources.buttonClicked);

            for (int i = 0; i < 10; i++)
            { // create panel of buttons with numbers
                this.numbers[i] = new System.Windows.Forms.Button();
                numbers[i].BackgroundImage = (Image)Properties.Resources.butonN;
                numbers[i].BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                numbers[i].Font = new System.Drawing.Font("Century", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                numbers[i].Margin = new System.Windows.Forms.Padding(0);
                numbers[i].Name = "button" + i;
                numbers[i].Size = new System.Drawing.Size(35, 35);
                numbers[i].TabIndex = i;
                numbers[i].Text = "" + i;
                numbers[i].UseVisualStyleBackColor = true;
                numbers[i].Click += new System.EventHandler(numbers_onClick);
            }
            for (int i = 0; i < 6; i++)
            { // creates labels that will be used to display options on the "machiece screen"
                this.text[i] = new System.Windows.Forms.Label();
                this.text[i].AutoSize = false;
                text[i].BackgroundImage = (Image)Properties.Resources.blue_button;
                text[i].BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
                this.text[i].Font = new System.Drawing.Font("Arial Narrow", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                this.text[i].Name = "text" + i;
                this.text[i].Size = new System.Drawing.Size(115, 25);
                this.text[i].TabIndex = i;
                this.text[i].Visible = false;
            }
            // all objects on their places
            numbers[1].Location = new System.Drawing.Point(7, 7);
            numbers[2].Location = new System.Drawing.Point(42, 7);
            numbers[3].Location = new System.Drawing.Point(77, 7);
            numbers[4].Location = new System.Drawing.Point(7, 42);
            numbers[5].Location = new System.Drawing.Point(42, 42);
            numbers[6].Location = new System.Drawing.Point(77, 42);
            numbers[7].Location = new System.Drawing.Point(7, 77);
            numbers[8].Location = new System.Drawing.Point(42, 77);
            numbers[9].Location = new System.Drawing.Point(77, 77);
            numbers[0].Location = new System.Drawing.Point(42, 112);

            text[0].Location = new System.Drawing.Point(48, 48);
            text[1].Location = new System.Drawing.Point(48, 96);
            text[2].Location = new System.Drawing.Point(48, 147);
            text[3].Location = new System.Drawing.Point(220, 48);
            text[4].Location = new System.Drawing.Point(220, 96);
            text[5].Location = new System.Drawing.Point(220, 147);

            for (int i = 0; i < 10; i++)
            {
                panel2.Controls.Add(numbers[i]);
                if (i < 6) { panel2.Controls.Add(text[i]); }
            }
            this.panel1.Controls.Add(text[0]);
            this.panel1.Controls.Add(text[1]);
            this.panel1.Controls.Add(text[2]);
            this.panel1.Controls.Add(text[3]);
            this.panel1.Controls.Add(text[4]);
            this.panel1.Controls.Add(text[5]);

        }
        #endregion

        #region bottom button click
        // textBox - is the area where user can type accaunt number and PIN
        // this method cleans the area from text, so user can type
        private void textBox1_onClick(object sender, EventArgs e)
        {
            String s = ((TextBox)sender).Text;
            if (s.Equals("Insert your card here") || s.Equals("Enter your pin here"))
            {
                ((TextBox)sender).Text = "";
                if (state == 1)
                    textBox1.PasswordChar = '*';
            }
        }

        // this method belongs to number pad
        // anytype user click numbers, these numbers appear in the textbox
        private void numbers_onClick(object sender, EventArgs e)
        {
            sound.Play();
            String s = textBox1.Text;
            if (s.Equals("Insert your card here") || s.Equals("Enter your pin here") || s.Equals("Enter here"))
            {
                textBox1.Text = "";
                if (state == 1)
                    textBox1.PasswordChar = '*';
            }
            if (textBox1.Text.Length < 10) // maximum length of the string inside textbox is restricted
                textBox1.Text = textBox1.Text + ((Button)sender).Text;
        }

        // button CLEAN was pressed. Textbox area is cleaned
        private void button17_Click(object sender, EventArgs e)
        {
            sound.Play();
            textBox1.Text = "";
        }

        //button CANCEL was pressed. ATM is ready for new client
        private void button15_Click(object sender, EventArgs e)
        {
            sound.Play();
            state = 0;
            textBox1.Text = "Insert your card here";
            activeAccount = null;
        }

        // I think we can enable keyboard at all if you want
        // but by now I did that pressing ENTER on keyboard insude textbox and by mose has the same effect 
        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                nextStep();
            }
        }

        // Green button with ENTER was pressed
        private void button18_Click(object sender, EventArgs e)
        {
            sound.Play();
            nextStep();
        }

        #endregion

        #region button and state control

        // check provided by user data and proceed to the next step. Called by 2 methods above
        private void nextStep()
        {
            switch (state)
            {
                case 0:
                    activeAccount = findAccount(textBox1.Text);
                    if (activeAccount != null) // if account found, go to the next step
                    {
                        textBox1.Text = "Enter your pin here";
                        screen1();
                        pinTry = 4;
                    }
                    else // if account was not found, program stays in the same state
                    {
                        textBox1.Text = "Insert your card here";
                        text[4].Text = "Wrong card";
                    }
                    break;
                case 1:
                    try
                    {
                        int input = Convert.ToInt32(IsDigitsOnly(textBox1.Text));
                        if (activeAccount.checkPin(input)) // if entered pin is correct
                        {
                            textBox1.Text = "";
                            textBox1.PasswordChar = '\0';
                            screen2();

                        }
                        else // if pin is wrong
                        {
                            pinFail();
                        }
                    }
                    catch (Exception) // if pin is wrong
                    {
                        pinFail();
                    }
                    break;
                case 6:
                    //Take user input and deposit it into their acount
                    try
                    {
                        int depositValue = Convert.ToInt32(IsDigitsOnly(textBox1.Text));
                        if (depositValue != 0) //Checks if deposit value is 
                        {

                            int oldBal = activeAccount.getBalance();
                            Thread.Sleep(3000);
                            activeAccount.setBalance(oldBal + depositValue);
                            activeAccount.Unlock();
                            //Go to stage 2
                            textBox1.Text = "";
                            textBox1.PasswordChar = '\0';

                            screen2();
                        }
                        else
                        {
                            textBox1.Text = "Invalid value";
                        }

                    }
                    catch (Exception) // if user input causes error
                    {
                        textBox1.Text = "Invalid value";
                    }

                    break;
                case 7: 
                //read the amount of money provided by user to the taken from account 
                    int userCash = Convert.ToInt32(IsDigitsOnly(textBox1.Text));
                    if (userCash % 5 == 0) // amount must be devisible by 5
                    {
                        if (datarace)
                            takeCashTrue(userCash);
                        else takeCashFalse(userCash);
                    }
                    else
                    {
                        textBox1.Text = "";
                        label1.Text = "Enter Amount\n Invalid number";
                    }
                        break;
                default:
                    textBox1.Text = "";
                    break;
            }
        }

        // inform user if PIN was typed wrong and counts how many chances the user has to provide correct PIN
        private void pinFail()
        {
            textBox1.Text = "Enter your pin here";
            textBox1.PasswordChar = '\0';
            // text[4].Text = "Wrong PIN";
            pinTry--;
            label1.Text = "Please, enter your PIN number\n" + pinTry + " try left";
            if (pinTry == 0)
            { // if user provides 4 wrong PINs, the account is closed and user is sent to the first, main ATM screen
                activeAccount = null;
                textBox1.Text = "Insert your card here";
                try
                {
                    label1.Text = "You failed your 4 chances\n" + "";
                    label1.Update();
                    Thread.Sleep(2000);
                }
                catch (Exception) { }
                label1.Text = "Please, insert your card by typing account number";
                state = 0;
                text[5].Visible = false;
            }
        }

        /*
         *    this method promts for the input of an account number
         *    the string input is then converted to an int
         *    a for loop is used to check the enterd account number
         *    against those held in the account array
         *    if a match is found a referance to the match is returned
         *    if the for loop completest with no match we return null
         * 
         */
        private Account findAccount(String s)
        {
            if (s.Length != 0)
                try
                {
                    int input = Convert.ToInt32(IsDigitsOnly(s));

                    for (int i = 0; i < this.ac.Length; i++)
                    {
                        if (ac[i].getAccountNum() == input)
                        {
                            return ac[i];
                        }
                    }
                }
                catch (FormatException) { }
            return null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            sound.Play();
            //Top button (Text box 0)
            //Switch statement based off what stage it is
            switch(state)
            {
                case 0:
                        //Does nothing
                        break;
                case 1:
                        //Does nothing
                        break;
                case 2:
                    //Go to stage 3
                    screen3();
                        break;
                case 3:
                        if (datarace)
                            takeCashTrue(5);
                        else takeCashFalse(5);
                    break;
                case 4: 
                case 5:
                    // do nothing
                    break;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            sound.Play();
            //Second button (Text box 1)
            switch (state)
            {
                case 0:
                    //Does nothing
                    break;
                case 1:
                    //Does nothing
                    break;
                case 2:
                    //Go to stage 5
                    screen5();
                    break;
                case 3:
                    // proceed paying
                    if (datarace)
                        takeCashTrue(20);
                    else takeCashFalse(20);
                    break;
                case 4: // nothing
                    break;
                case 5:
                    screen3();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sound.Play();
            //Third button (text box 2)
            switch (state)
            {
                case 0:
                    //Does nothing
                    break;
                case 1:
                    //Does nothing
                    break;
                case 2:
                    //Go to stage 6
                    screen6();
                    break;
                case 3:
                    // proceed paying
                    if (datarace)
                        takeCashTrue(0);
                    else takeCashFalse(0);
                    break;
                case 4: 
                case 5:
                    screen2();
                    break;
                case 7:
                    screen3();
                    break;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            sound.Play();
            // Fourth button (text box 3)
            switch (state)
            {
                case 3:
                    // proceed paying
                    if (datarace)
                        takeCashTrue(10);
                    else takeCashFalse(10);
                    break;
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            sound.Play();
            //Fith button (text box 4)
            switch (state)
            {
                case 3:
                    // proceed paying
                    if (datarace)
                        takeCashTrue(50);
                    else takeCashFalse(50);
                    break;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            sound.Play();
            //Sixth button (text box 5)
            //Will always exit (except state 3)
            if (state == 3)
                screen2(); // goes back to the operations menu
            else if (state != 0)
            {
                screen0();
                textBox1.PasswordChar = '\0';
            }
            else
            {
                this.Close();
            }
        }
        #endregion

        #region take cash

        //allow user to take cash from the account with datarace. It receives as input amount of money to be taken
        private void takeCashTrue(int cash)
        {
            if (cash == 0) //if amount of money is equal to zero, user is prompt to specify valid amount
            {
                screen7();
            }
            else
            {
                textBox1.Text = "";
                if (activeAccount.getBalance() < cash) // checks if there is enought money to be taken
                {
                    activeAccount.Unlock(); // unlock the access to the balance 
                    setScreen4False();
                }
                else
                {
                    activeAccount.Unlock(); // unlock the access to the balance 
                    int oldBalance = activeAccount.getBalance();
                    activeAccount.Unlock(); // unlock access to the balance, that won't prevent datarace condition 
                    Thread.Sleep(1000); // sleep to give to a user chance to perform simultaneous work for another ATM thread
                    setScreen4True();
                    Thread.Sleep(2000);
                    activeAccount.setBalance(oldBalance - cash);
                }
                state=4;
            }
        }

        //allow user to take cash from the account without datarace. It receives as input amount of money to be taken
        private void takeCashFalse(int cash)
        {
            if (cash == 0)
            {
                screen7(); //if amount of money is equal to zero, user is prompt to specify valid amount
            }
            else
            {
                textBox1.Text = "";
                if (activeAccount.getBalance() < cash) // checks if there is enought money to be taken
                {
                    activeAccount.Unlock(); // unlock the access to the balance 
                    setScreen4False();
                }
                else
                {
                    activeAccount.Unlock(); // unlock the access to the balance 
                    int oldBalance = activeAccount.getBalance(); // get balance
                    Thread.Sleep(1000); // sleep to give to a user chance to perform simultaneous work for another ATM thread
                    setScreen4True(); 
                    Thread.Sleep(2000);
                    activeAccount.setBalance(oldBalance - cash);
                    activeAccount.Unlock(); // unlock the access to the balance after all changes were performed. Prevents datarace condition
                }
                state = 4;
            }
        }

        //partly set screen saying that money were taken successfully
        private void setScreen4True()
        {

            label1.Text = "Thanks, do not forget to take your money!";
            screen4();
        }

        //partly set screen saying that it was impossible to take money
        private void setScreen4False()
        {
            label1.Text = "Sorry, but you do not have enought money!";
            screen4();
        }
        #endregion

        #region screens
        //methods in this region set the screen for each state of the program
        
        //set screen that promts user to enter their account number
        private void screen0()
        {
            state = 0;
            for (int i = 0; i < 6; i++)
                text[i].Visible = false;
            textBox1.Text = "Insert your card here";
            label1.Text = "Please, insert your card by typing account number";
            label1.Visible = true;

        }

        //set screen that promts user to enter their PIN
        private void screen1()
        {
            state = 1;
            textBox1.Text = "Enter your pin here";
            label1.Text = "Please, enter your PIN";
            text[5].Text = "Exit";
            text[5].Visible = true;
        }

        //set screen that ask user what operation they want to perform
        private void screen2()
        {
            state = 2;
            label1.Visible = false;
            text[0].Text = "Take cash";
            text[1].Text = "See balance";
            text[2].Text = "Put money";
            text[5].Text = "Exit";
            text[0].Visible = true;
            text[1].Visible = true;
            text[2].Visible = true;
        }

        //set screen that asks user for the amount of money to be taken
        private void screen3()
        {
            state = 3;
            label1.Visible = false;
            text[0].Visible = true;
            text[1].Visible = true;
            text[3].Visible = true;
            text[4].Visible = true;
            text[0].Text = "£5";
            text[1].Text = "£20";
            text[2].Text = "Other";
            text[3].Text = "£10";
            text[4].Text = "£50";
            text[5].Text = "Back";
        }

        //partly set screen that says to user if money were taken successfully
        private void screen4()
        {
            state = 4;
            text[0].Visible = false;
            text[1].Visible = false;
            text[3].Visible = false;
            text[4].Visible = false;
            label1.Visible = true;
            text[2].Text = "Back";
            text[5].Text = "Exit";

        }

        //set screen that shows the balance in the account
        private void screen5()
        {
            state = 5;
            text[0].Visible = false;
            label1.Text = "Your Balance is £" + activeAccount.getBalance().ToString();
            activeAccount.Unlock();
            label1.Visible = true;
            text[1].Text = "Withdraw Money";
            text[2].Text = "Back";
        }

        //set screen that promts user to specify the amount of money to put on the account
        private void screen6()
        {
            state = 6;
            label1.Text = "Enter Amount Deposited";
            textBox1.Text = "Enter here";
            label1.Visible = true;
            text[0].Visible = false;
            text[1].Visible = false;
            text[2].Visible = false;
        }

        //set screen that promts user to specify the amount of money to be taken from the account
        private void screen7()
        {
            label1.Text = "Enter Amount";
            textBox1.Text = "Enter here";
            label1.Visible = true;
            text[0].Visible = false;
            text[1].Visible = false;
            text[3].Visible = false;
            text[4].Visible = false;
            text[2].Text = "Back";
            text[5].Text = "Exit";
            state = 7;
        }

        #endregion

        string IsDigitsOnly(string str) //For checking if a string only contains numbers to fix error. Slightly modified from http://stackoverflow.com/questions/7461080/fastest-way-to-check-if-string-contains-only-digits
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return "0";
                //Checks through the string, if any chars are not numbers converts the entire string to the number 0
            }

            return str;
        }

    }
}


