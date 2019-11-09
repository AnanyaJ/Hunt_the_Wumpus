using System;
using System.Collections.Generic;
using System.IO;

	public class Trivia
	{
        // constants
        private readonly double MIN_PORTION_QUESTIONS_ANSWERED_CORRECTLY = 0.6; // player must answer at least 60% of questions rght
        private readonly string QUESTIONS_FILE = "CompiledAllQuestions.txt";
        private readonly string ANSWER_CHOICES_FILE = "CompiledAllChoices.txt";
        private readonly string CORRECT_ANSWERS_FILE = "CompiledAllCorrectAnswers.txt";

        // instance variables
        private int numTriviaQuestions;
		private List<int> usedQuestions;
		private List<string> questionList;
		private List<string> answerChoiceList;
		private List<string> answerList;

        // create questions, choices, answers lists
		public Trivia()	
		{
            try
            {
                StreamReader questions = new StreamReader(QUESTIONS_FILE);
                StreamReader answerChoices = new StreamReader(ANSWER_CHOICES_FILE);
                StreamReader correctAnswers = new StreamReader(CORRECT_ANSWERS_FILE);
                usedQuestions = new List<int>();
                questionList = new List<string>();
                answerChoiceList = new List<string>();
                answerList = new List<string>();

                while (!questions.EndOfStream)
                {
                    questionList.Add(questions.ReadLine());
                    answerChoiceList.Add(answerChoices.ReadLine());
                    answerList.Add(correctAnswers.ReadLine());
                }

                numTriviaQuestions = questionList.Count;
            }
            catch (FileNotFoundException exception) // exception handling, file missing
            {
                Console.WriteLine("File Missing", exception);
                throw new FileNotFoundException(@"[file missing]", exception);
            }
        }

        // accessor methods

		// returns a random question number
		// ensures that the random number has not already been used
		public int GetRandom()
		{
			Random r = new Random();
			int question = r.Next(numTriviaQuestions);

            // if there's still a question that hasn't been used...
            if (usedQuestions.Count < numTriviaQuestions)
            {
                while (usedQuestions.Contains(question))
                {
                    question = r.Next(numTriviaQuestions);
                }
            }
        
			usedQuestions.Add(question);
			return question;
		}
        
		//Get the Question
		public string GetQuestion(int x)
		{
			return questionList[x];
		}

		//Get the Answer Choices
		public string GetChoices(int x)
		{
			return answerChoiceList[x];
		}

		//Get the correct answer
		public string GetAnswer(int x)
		{
			return answerList[x];
		}
        
		//Verify if the user was successful in moving past the obstacle
        // true if passed, false otherwise
		public bool PassedTrivia(int totalQuestions, int answeredCorrect)
		{
            return ((double) answeredCorrect)/totalQuestions >= MIN_PORTION_QUESTIONS_ANSWERED_CORRECTLY;
		}

        // returns number of extra questions player answered right
        public int ExtraTriviaQuestionsAnswered(int totalQuestions, int answeredCorrect)
        {
            return Math.Max(0, answeredCorrect - (int) Math.Ceiling(MIN_PORTION_QUESTIONS_ANSWERED_CORRECTLY * totalQuestions));
        }
}