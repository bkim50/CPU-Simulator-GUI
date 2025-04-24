/*
 *  Project Two: CPU Scheduler
 *  Name: Brien Kim
 *  Course: CS 3502 Section W03
 *  Net ID: bkim50
 *  
 *  For this project, two advanced scheduling algorithms are added into this file (Algorithms.cs):
 *  - Shortest Remaining Time First (SRTF): preemptive version of Shortest Job First (SJF) 
 *  - Highest Response Ratio Next (HRRN): non-preemptive algorithm
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace CpuSchedulingWinForms
{
    public static class Algorithms
    {


        /* Algorithms for the Project Two start from here */


        // declare the Process class to efficienty manage each process
        private class Process
        {
            // attributes that represent basic information for each process
            public int id;
            static int tracking_id;
            public int arrival_time;
            public int burst_time;

            // attribute that represent information for calculation and comparison
            public int remaining_burst_time;
            public int completion_time;
            // turnaround_time = completion_time - arrival_time
            public int turnaround_time;
            // waiting_time = turnaround_time - burst_time
            public int waiting_time;

            // overloaded constructor, take required inputs from a user
            public Process(int arrival_time, int burst_time)
            {
                this.id = ++tracking_id;
                this.arrival_time = arrival_time;
                this.burst_time = burst_time;
                this.remaining_burst_time = burst_time;
            }
        }
        
        /*
         *  Shortest Remaining Time First(SRTF) Algorithm:
         *  a preemptive scheduling algorithm that prioritizes the process
         *  with the shortest remaining time (remaining burst time)
         */
        public static void srtfAlgorithm(string userInput)
        {
            // start to measure CPU utilization
            PerformanceCounter cpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            dynamic startValue = cpu.NextValue();

            // initialize number of process and an array to store processes
            int number_of_process = Convert.ToInt32(userInput);
            Process[] waiting_processes = new Process[number_of_process];

            // retrieve a user's confirmation to operate SRTF algorithm
            DialogResult result = MessageBox.Show("Shortest Remaining Time First Scheduling ", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
           
            // if a user does not confirm, then terminate the method
            if (result != DialogResult.Yes)
            {
                return;
            }

            // for number of process, prompt a user for arrival time and burst time for each process
            // then add each process to the array
            for (int i = 0; i < number_of_process; i++)
            {
                string arrival_time = Microsoft.VisualBasic.Interaction.InputBox("Enter Arrival Time: ", "Arrival Time for P" + (i + 1), "", -1, -1);
                string burst_time = Microsoft.VisualBasic.Interaction.InputBox("Enter Burst Time: ", "Burst Time for P" + (i + 1), "", -1, -1);

                waiting_processes[i] = new Process(Convert.ToInt32(arrival_time), Convert.ToInt32(burst_time));
            }

            // sort the array from earliest arrival time to latest arrival time
            // - if there are multiple processes with the same arrival time,
            //   then still these will be sorted by burst time
            // - if all processes have the same arrival time and the same burst time,
            //   then nothing will be sorted within the array
            waiting_processes = waiting_processes.OrderBy(process => process.arrival_time)
                                                 .ThenBy(process => process.burst_time)
                                                 .ToArray();

            // track number of completed process and time past
            int number_of_completed_process = 0;
            int current_time = 0;

            // while there is an un-completed process, continue performing the algorithm
            while (number_of_completed_process < number_of_process)
            {
                // initially, there is no current process due to the possibility of an idle period 
                int current_process_index = -1;

                // for the number of process, select a process with the shortest remaining burst time
                // - if there is an availble process (while there is no current process),
                //   then re-initialze the process as the current process
                // - if there is an un-completed process whose arrival time is within the current time,
                //   whose remaining burst time is greater than 0 and less than the current process' remaining burst time,
                //   then swap the current process with the process
                for (int i = 0; i < waiting_processes.Length; i++)
                {
                    Process process = waiting_processes[i];

                    if (process.arrival_time <= current_time && 
                        process.remaining_burst_time > 0 &&
                        (current_process_index == -1 ||
                        process.remaining_burst_time < waiting_processes[current_process_index].remaining_burst_time))
                    {
                        current_process_index = i;
                    }
                }

                // if there is an idle period (current_process_index == -1),
                // then skip the below operations (by continue keyword) and increment the current time by 1
                if (current_process_index == -1)
                {
                    current_time++;
                    continue;
                }

                // otherwise (if there is no idle period),
                // - increment the current time by 1
                // - decrement the current process' remaining burst time by 1
                current_time++;
                Process current_process = waiting_processes[current_process_index];

                // if calculated remaining burst time is 0 (meaning the process is completed),
                // then re-initialize its completion time and calculat its turnaround time and waiting time
                // lastly, increment the number of completed process by 1
                if (--current_process.remaining_burst_time == 0)
                {
                    current_process.completion_time = current_time;

                    // turnaround_time = completion_time - arrival_time
                    current_process.turnaround_time = current_process.completion_time - current_process.arrival_time;
                    // waiting_time = turnaround_time - burst_time
                    current_process.waiting_time = current_process.turnaround_time - current_process.burst_time;

                    number_of_completed_process++;
                }
            }

            // after all processes are completed,
            // calculate average turnaround time (ATT) and average waiting time (AWT)
            double average_turnaround_time = 0;
            double average_waiting_time = 0;

            foreach (Process process in waiting_processes)
            {
                average_turnaround_time += process.turnaround_time;
                average_waiting_time += process.waiting_time;
            }

            average_turnaround_time /= number_of_process;
            average_waiting_time /= number_of_process;

            // calculate throughput (Processes per Second)
            // - throughput = [total number of completed processes] / [overall completed time]
            // - overall, "current time" will be "completed time"
            // - since the "current time" is in "milli-second", convert it to "second" by divide 1,000
            double throughput = number_of_completed_process / ((double) current_time / 1000);

            // display results:
            // - average waiting time (milli-seconds)
            // - average turnaround time (milli-seconds)
            // - CPU Utilization (%)
            // - Throughput (processes / second)
            MessageBox.Show("** Shortest Remaining Time First (SRTF) with "+ number_of_process + " processes **\n\n"
                            + "Average Waiting Time (AWT) = " + average_waiting_time.ToString("0.0##") + " ms\n\n" 
                            + "Average Turnaround Time (ATT) = " + average_turnaround_time.ToString("0.0##") + " ms\n\n"
                            + "CPU Utilization = " + ((double) cpu.NextValue()).ToString("0.0##") + " %\n\n"
                            + "Throughput = " + throughput.ToString("0.0##") + " processes/second\n"
                            , "Shortest Remaining Time First (SRTF) Result", MessageBoxButtons.OK, MessageBoxIcon.None);
        }



        /*
         *  Highest Response Ratio Next (HRRN) Algorithm:
         *  a non-preemptive scheduling algorithm that prioritizes the process
         *  based on its response ratio 
         *  = ([waiting time] + [remaining burst time]) / [remaining burst tiem].
         */
        public static void hrrnAlgorithm(string userInput)
        {
            // start to measure CPU utilization
            PerformanceCounter cpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            dynamic startValue = cpu.NextValue();

            // initialize number of process and an array to store processes
            int number_of_process = Convert.ToInt16(userInput);
            Process[] waiting_processes = new Process[number_of_process];

            // retrieve a user's confirmation to operate SRTF algorithm
            DialogResult result = MessageBox.Show("Highest Response Ratio Next Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            // if a user does not confirm, then terminate the method
            if (result != DialogResult.Yes)
            {
                return;
            }

            // for number of process, prompt a user for arrival time and burst time for each process
            // then add each process to the array
            for (int i = 0; i < number_of_process; i++)
            {
                string arrival_time = Microsoft.VisualBasic.Interaction.InputBox("Enter Arrival Time: ", "Arrival Time for P" + (i + 1), "", -1, -1);
                string burst_time = Microsoft.VisualBasic.Interaction.InputBox("Enter Burst Time: ", "Burst Time for P" + (i + 1), "", -1, -1);

                waiting_processes[i] = new Process(Convert.ToInt32(arrival_time), Convert.ToInt32(burst_time));
            }

            // sort the array from earliest arrival time to latest arrival time
            // - if there are multiple processes with the same arrival time,
            //   then still these will be sorted by burst time
            // - if all processes have the same arrival time and the same burst time,
            //   then nothing will be sorted within the array
            waiting_processes = waiting_processes.OrderBy(process => process.arrival_time)
                                                 .ThenBy(process => process.burst_time)
                                                 .ToArray();

            // track number of completed process and time past
            int number_of_completed_process = 0;
            int current_time = 0;

            // while there is an un-completed process, continue performing the algorithm
            while (number_of_completed_process < number_of_process)
            {
                // initially, there is no current process due to the possibility of an idle period 
                int current_process_index = -1;

                // for the number of process, select a process with the highest response ratio
                // - if there is an availble process (while there is no current process),
                //   then re-initialze the process as the current process
                // - if there is an un-completed process whose arrival time is within the current time,
                //   whose response ratio is greater than the current process' response ratio,
                //   then swap the current process with the process
                for (int i = 0; i < number_of_process; i++)
                {
                    Process process = waiting_processes[i];

                    // since HRRN is non-preemptive, the waiting time for each process is [current time] - [arrival time]
                    int current_waiting_time = current_time - process.arrival_time;
                    // response ratio = ([waiting time] + [remaining burst time]) / [remaining burst tiem]
                    double response_ratio = (double)(current_waiting_time + process.remaining_burst_time) / process.remaining_burst_time;

                    if (process.arrival_time <= current_time && 
                        process.remaining_burst_time > 0 &&
                        (current_process_index == -1 ||
                         response_ratio > (double) ((current_time - waiting_processes[current_process_index].arrival_time) + waiting_processes[current_process_index].remaining_burst_time) / waiting_processes[current_process_index].remaining_burst_time))
                    {
                        current_process_index = i;
                    }
                }

                // if there is an idle period (current_process_index == -1),
                // then skip the below operations (by continue keyword) and increment the current time by 1
                if (current_process_index == -1)
                {
                    current_time++;
                    continue;
                }

                // otherwise (if there is no idle period),
                // since HRRN is non-premptive,
                // - increment the current time by 1 until the current process is completed (remaining_burst_time == 0)
                // - initialze the current process' completion time
                // - increment the number of completed process by 1
                Process current_process = waiting_processes[current_process_index];

                while (current_process.remaining_burst_time-- > 0)
                {
                    current_time++;
                }

                current_process.completion_time = current_time;

                // turnaround_time = completion_time - arrival_time
                current_process.turnaround_time = current_process.completion_time - current_process.arrival_time;
                // waiting_time = turnaround_time - burst_time
                current_process.waiting_time = current_process.turnaround_time - current_process.burst_time;

                number_of_completed_process++;

            }

            // after all processes are completed,
            // calculate average turnaround time (ATT) and average waiting time (AWT)
            double average_turnaround_time = 0;
            double average_waiting_time = 0;

            foreach (Process process in waiting_processes)
            {
                average_turnaround_time += process.turnaround_time;
                average_waiting_time += process.waiting_time;
            }

            average_turnaround_time /= number_of_process;
            average_waiting_time /= number_of_process;

            // calculate throughput (Processes per Second)
            // - throughput = [total number of completed processes] / [overall completed time]
            // - overall, "current time" will be "completed time"
            // - since the "current time" is in "milli-second", convert it to "second" by divide 1,000
            double throughput = number_of_completed_process / ((double)current_time / 1000);

            // display results:
            // - average waiting time (milli-seconds)
            // - average turnaround time (milli-seconds)
            // - CPU Utilization (%)
            // - Throughput (processes / second)
            MessageBox.Show("** Highest Response Ratio Next (HRRN) with " + number_of_process + " processes **\n\n"
                            + "Average Waiting Time (AWT) = " + average_waiting_time.ToString("0.0##") + " ms\n\n"
                            + "Average Turnaround Time (ATT) = " + average_turnaround_time.ToString("0.0##") + " ms\n\n"
                            + "CPU Utilization = " + ((double) cpu.NextValue()).ToString("0.0##") + " %\n\n"
                            + "Throughput = " + throughput.ToString("0.0##") + " processes/second\n"
                            , "Highest Response Ratio Next (HRRN) Result", MessageBoxButtons.OK, MessageBoxIcon.None);
        }


        /* Algorithms for the Project Two Ends */


        /*
         *  initially,four basic CPU scheduling algorithms are implemented in this file:
         *  - First Come, First Served (FCFS)
         *  - Shortest Job First (SJF)
         *  - Priority Scheduling
         *  - Round Robin (RR)
         *  
         *  modify those initial four algorithms to properly compare with newly added algorithms
         *  - add CPU utilization calculatiion for each algorithm
         *  - add turnaround time (= waiting time + burst time) for FCFS, SJF, and Priority Scheduling
         *  - re-format result display for each algorithm
         */
        public static void fcfsAlgorithm(string userInput)
        {
            // start to measure CPU utilization
            PerformanceCounter cpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            dynamic startValue = cpu.NextValue();

            int np = Convert.ToInt16(userInput);
            int npX2 = np * 2;

            // add new array for turnaround time
            double[] turnaround_time_array = new double[np];

            double[] bp = new double[np];
            double[] wtp = new double[np];
            string[] output1 = new string[npX2];
            double twt = 0.0, awt; 
            int num;

            DialogResult result = MessageBox.Show("First Come First Serve Scheduling ", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                for (num = 0; num <= np - 1; num++)
                {
                    string input =
                    Microsoft.VisualBasic.Interaction.InputBox("Enter Burst time: ",
                                                       "Burst time for P" + (num + 1),
                                                       "",
                                                       -1, -1);

                    bp[num] = Convert.ToInt64(input);

                    //var input = Console.ReadLine();
                    //bp[num] = Convert.ToInt32(input);
                }

                for (num = 0; num <= np - 1; num++)
                {
                    if (num == 0)
                    {
                        wtp[num] = 0;

                        // turnaround time = [waiting time] + [burst time]
                        turnaround_time_array[num] = wtp[num] + bp[num];
                    }
                    else
                    {
                        wtp[num] = wtp[num - 1] + bp[num - 1];

                        // turnaround time = [waiting time] + [burst time]
                        turnaround_time_array[num] = wtp[num] + bp[num];
                    }
                }
                for (num = 0; num <= np - 1; num++)
                {
                    twt = twt + wtp[num];
                }
                awt = twt / np;


                // calculate average turnaround time (ATT)
                double average_turnaround_time = 0.0;

                foreach (int turnaround_time in turnaround_time_array)
                {
                    average_turnaround_time += turnaround_time;
                }

                average_turnaround_time /= np;

                // calculate throughput (Processes per Second)
                // - throughput = [total number of completed processes] / [overall completed time]
                // - overall, "last process' turnaround time" will be "completed time" since this algorithm does not take arrival time for each process
                // - since the "turnaround time" is in "milli-second", convert it to "second" by divide 1,000
                double throughput = np / ((double) turnaround_time_array[turnaround_time_array.Length - 1] / 1000);

                // display results:
                // - average waiting time (milli-seconds)
                // - average turnaround time (milli-seconds)
                // - CPU Utilization (%)
                // - Throughput (processes / second)
                MessageBox.Show("** First Come, First Served (FCFS) with " + np + " processes **\n\n"
                                + "Average Waiting Time (AWT) = " + awt.ToString("0.0##") + " ms\n\n"
                                + "Average Turnaround Time (ATT) = " + average_turnaround_time.ToString("0.0##") + " ms\n\n"
                                + "CPU Utilization = " + ((double) cpu.NextValue()).ToString("0.0##") + " %\n\n"
                                + "Throughput = " + throughput.ToString("0.0##") + " processes/second\n"
                                , "Highest Response Ratio Next (HRRN) Result", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else if (result == DialogResult.No)
            {
                //this.Hide();
                //Form1 frm = new Form1();
                //frm.ShowDialog();
            }
        }

        public static void sjfAlgorithm(string userInput)
        {
            // start to measure CPU utilization
            PerformanceCounter cpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            dynamic startValue = cpu.NextValue();

            int np = Convert.ToInt16(userInput);

            // add new array for turnaround time
            double[] turnaround_time_array = new double[np];

            double[] bp = new double[np];
            double[] wtp = new double[np];
            double[] p = new double[np];
            double twt = 0.0, awt; 
            int x, num;
            double temp = 0.0;
            bool found = false;

            DialogResult result = MessageBox.Show("Shortest Job First Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                for (num = 0; num <= np - 1; num++)
                {
                    string input =
                        Microsoft.VisualBasic.Interaction.InputBox("Enter burst time: ",
                                                           "Burst time for P" + (num + 1),
                                                           "",
                                                           -1, -1);

                    bp[num] = Convert.ToInt64(input);
                }
                for (num = 0; num <= np - 1; num++)
                {
                    p[num] = bp[num];
                }
                for (x = 0; x <= np - 2; x++)
                {
                    for (num = 0; num <= np - 2; num++)
                    {
                        if (p[num] > p[num + 1])
                        {
                            temp = p[num];
                            p[num] = p[num + 1];
                            p[num + 1] = temp;
                        }
                    }
                }
                for (num = 0; num <= np - 1; num++)
                {
                    if (num == 0)
                    {
                        for (x = 0; x <= np - 1; x++)
                        {
                            if (p[num] == bp[x] && found == false)
                            {
                                wtp[num] = 0;
                                //MessageBox.Show("Waiting time for P" + (x + 1) + " = " + wtp[num], "Waiting time:", MessageBoxButtons.OK, MessageBoxIcon.None);
                                
                                // turnaround time = [waiting time] + [burst time]
                                turnaround_time_array[num] = wtp[num] + bp[x];

                                bp[x] = 0;
                                found = true;
                            }
                        }
                        found = false;
                    }
                    else
                    {
                        for (x = 0; x <= np - 1; x++)
                        {
                            if (p[num] == bp[x] && found == false)
                            {
                                wtp[num] = wtp[num - 1] + p[num - 1];
                                //MessageBox.Show("Waiting time for P" + (x + 1) + " = " + wtp[num], "Waiting time", MessageBoxButtons.OK, MessageBoxIcon.None);

                                // turnaround time = [waiting time] + [burst time]
                                turnaround_time_array[num] = wtp[num] + bp[x];

                                bp[x] = 0;
                                found = true;
                            }
                        }
                        found = false;
                    }
                }
                for (num = 0; num <= np - 1; num++)
                {
                    twt = twt + wtp[num];
                }
                awt = twt / np;

                // calculate average turnaround time (ATT)
                double average_turnaround_time = 0.0;

                foreach (int turnaround_time in turnaround_time_array)
                {
                    average_turnaround_time += turnaround_time;
                }

                average_turnaround_time /= np;

                // calculate throughput (Processes per Second)
                // - throughput = [total number of completed processes] / [overall completed time]
                // - overall, "last process' turnaround time" will be "completed time" since this algorithm does not take arrival time for each process
                // - since the "turnaround time" is in "milli-second", convert it to "second" by divide 1,000
                double throughput = np / ((double)turnaround_time_array[turnaround_time_array.Length - 1] / 1000);

                // display results:
                // - average waiting time (milli-seconds)
                // - average turnaround time (milli-seconds)
                // - CPU Utilization (%)
                // - Throughput (processes / second)
                MessageBox.Show("** Shortest Job First (SJF) with " + np + " processes **\n\n"
                                + "Average Waiting Time (AWT) = " + awt.ToString("0.0##") + " ms\n\n"
                                + "Average Turnaround Time (ATT) = " + average_turnaround_time.ToString("0.0##") + " ms\n\n"
                                + "CPU Utilization = " + ((double)cpu.NextValue()).ToString("0.0##") + " %\n\n"
                                + "Throughput = " + throughput.ToString("0.0##") + " processes/second\n"
                                , "Shortest Job First (SJF) Result", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }

        public static void priorityAlgorithm(string userInput)
        {
            // start to measure CPU utilization
            PerformanceCounter cpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            dynamic startValue = cpu.NextValue();

            int np = Convert.ToInt16(userInput);

            // add new array for turnaround time
            double[] turnaround_time_array = new double[np];

            DialogResult result = MessageBox.Show("Priority Scheduling ", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                double[] bp = new double[np];
                double[] wtp = new double[np + 1];
                int[] p = new int[np];
                int[] sp = new int[np];
                int x, num;
                double twt = 0.0;
                double awt;
                int temp = 0;
                bool found = false;
                for (num = 0; num <= np - 1; num++)
                {
                    string input =
                        Microsoft.VisualBasic.Interaction.InputBox("Enter burst time: ",
                                                           "Burst time for P" + (num + 1),
                                                           "",
                                                           -1, -1);

                    bp[num] = Convert.ToInt64(input);
                }
                for (num = 0; num <= np - 1; num++)
                {
                    string input2 =
                        Microsoft.VisualBasic.Interaction.InputBox("Enter priority: ",
                                                           "Priority for P" + (num + 1),
                                                           "",
                                                           -1, -1);

                    p[num] = Convert.ToInt16(input2);
                }
                for (num = 0; num <= np - 1; num++)
                {
                    sp[num] = p[num];
                }
                for (x = 0; x <= np - 2; x++)
                {
                    for (num = 0; num <= np - 2; num++)
                    {
                        if (sp[num] > sp[num + 1])
                        {
                            temp = sp[num];
                            sp[num] = sp[num + 1];
                            sp[num + 1] = temp;
                        }
                    }
                }
                for (num = 0; num <= np - 1; num++)
                {
                    if (num == 0)
                    {
                        for (x = 0; x <= np - 1; x++)
                        {
                            if (sp[num] == p[x] && found == false)
                            {
                                wtp[num] = 0;
                                //MessageBox.Show("Waiting time for P" + (x + 1) + " = " + wtp[num], "Waiting time", MessageBoxButtons.OK);
                                //Console.WriteLine("\nWaiting time for P" + (x + 1) + " = " + wtp[num]);

                                // turnaround time = [waiting time] + [burst time]
                                turnaround_time_array[num] = wtp[num] + bp[x];

                                temp = x;
                                p[x] = 0;
                                found = true;
                            }
                        }
                        found = false;
                    }
                    else
                    {
                        for (x = 0; x <= np - 1; x++)
                        {
                            if (sp[num] == p[x] && found == false)
                            {
                                wtp[num] = wtp[num - 1] + bp[temp];
                                //MessageBox.Show("Waiting time for P" + (x + 1) + " = " + wtp[num], "Waiting time", MessageBoxButtons.OK);
                                //Console.WriteLine("\nWaiting time for P" + (x + 1) + " = " + wtp[num]);

                                // turnaround time = [waiting time] + [burst time]
                                turnaround_time_array[num] = wtp[num] + bp[x];

                                temp = x;
                                p[x] = 0;
                                found = true;
                            }
                        }
                        found = false;
                    }
                }
                for (num = 0; num <= np - 1; num++)
                {
                    twt = twt + wtp[num];
                }
                awt = twt / np;
                //MessageBox.Show("Average waiting time for " + np + " processes" + " = " + (awt = twt / np) + " sec(s)", "Average waiting time", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Console.WriteLine("\n\nAverage waiting time: " + (awt = twt / np));
                //Console.ReadLine();

                // calculate average turnaround time (ATT)
                double average_turnaround_time = 0.0;

                foreach (int turnaround_time in turnaround_time_array)
                {
                    average_turnaround_time += turnaround_time;
                }

                average_turnaround_time /= np;

                // calculate throughput (Processes per Second)
                // - throughput = [total number of completed processes] / [overall completed time]
                // - overall, "last process' turnaround time" will be "completed time" since this algorithm does not take arrival time for each process
                // - since the "turnaround time" is in "milli-second", convert it to "second" by divide 1,000
                double throughput = np / ((double)turnaround_time_array[turnaround_time_array.Length - 1] / 1000);

                // display results:
                // - average waiting time (milli-seconds)
                // - average turnaround time (milli-seconds)
                // - CPU Utilization (%)
                // - Throughput (processes / second)
                MessageBox.Show("** Shortest Job First (SJF) with " + np + " processes **\n\n"
                                + "Average Waiting Time (AWT) = " + awt.ToString("0.0##") + " ms\n\n"
                                + "Average Turnaround Time (ATT) = " + average_turnaround_time.ToString("0.0##") + " ms\n\n"
                                + "CPU Utilization = " + ((double)cpu.NextValue()).ToString("0.0##") + " %\n\n"
                                + "Throughput = " + throughput.ToString("0.0##") + " processes/second\n"
                                , "Shortest Job First (SJF) Result", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            else
            {
                //this.Hide();
            }
        }

        public static void roundRobinAlgorithm(string userInput)
        {
            // start to measure CPU utilization
            PerformanceCounter cpu = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            dynamic startValue = cpu.NextValue();

            int np = Convert.ToInt16(userInput);
            int i, counter = 0;
            double total = 0.0;
            double timeQuantum;
            double waitTime = 0, turnaroundTime = 0;
            double averageWaitTime, averageTurnaroundTime;
            double[] arrivalTime = new double[10];
            double[] burstTime = new double[10];
            double[] temp = new double[10];
            int x = np;

            DialogResult result = MessageBox.Show("Round Robin Scheduling", "", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                for (i = 0; i < np; i++)
                {
                    string arrivalInput =
                            Microsoft.VisualBasic.Interaction.InputBox("Enter arrival time: ",
                                                               "Arrival time for P" + (i + 1),
                                                               "",
                                                               -1, -1);

                    arrivalTime[i] = Convert.ToInt64(arrivalInput);

                    string burstInput =
                            Microsoft.VisualBasic.Interaction.InputBox("Enter burst time: ",
                                                               "Burst time for P" + (i + 1),
                                                               "",
                                                               -1, -1);

                    burstTime[i] = Convert.ToInt64(burstInput);

                    temp[i] = burstTime[i];
                }
                string timeQuantumInput =
                            Microsoft.VisualBasic.Interaction.InputBox("Enter time quantum: ", "Time Quantum",
                                                               "",
                                                               -1, -1);

                timeQuantum = Convert.ToInt64(timeQuantumInput);
                Helper.QuantumTime = timeQuantumInput;

                for (total = 0, i = 0; x != 0;)
                {
                    if (temp[i] <= timeQuantum && temp[i] > 0)
                    {
                        total = total + temp[i];
                        temp[i] = 0;
                        counter = 1;
                    }
                    else if (temp[i] > 0)
                    {
                        temp[i] = temp[i] - timeQuantum;
                        total = total + timeQuantum;
                    }
                    if (temp[i] == 0 && counter == 1)
                    {
                        x--;
                        //printf("nProcess[%d]tt%dtt %dttt %d", i + 1, burst_time[i], total - arrival_time[i], total - arrival_time[i] - burst_time[i]);
                        //MessageBox.Show("Turnaround time for Process " + (i + 1) + " : " + (total - arrivalTime[i]), "Turnaround time for Process " + (i + 1), MessageBoxButtons.OK);
                        //MessageBox.Show("Wait time for Process " + (i + 1) + " : " + (total - arrivalTime[i] - burstTime[i]), "Wait time for Process " + (i + 1), MessageBoxButtons.OK);
                        turnaroundTime = (turnaroundTime + total - arrivalTime[i]);
                        waitTime = (waitTime + total - arrivalTime[i] - burstTime[i]);                        
                        counter = 0;
                    }
                    if (i == np - 1)
                    {
                        i = 0;
                    }
                    else if (arrivalTime[i + 1] <= total)
                    {
                        i++;
                    }
                    else
                    {
                        i = 0;
                    }
                }
                averageWaitTime = Convert.ToInt64(waitTime * 1.0 / np);
                averageTurnaroundTime = Convert.ToInt64(turnaroundTime * 1.0 / np);
                //MessageBox.Show("Average wait time for " + np + " processes: " + averageWaitTime + " sec(s)", "", MessageBoxButtons.OK);
                //MessageBox.Show("Average turnaround time for " + np + " processes: " + averageTurnaroundTime + " sec(s)", "", MessageBoxButtons.OK);

                // calculate throughput (Processes per Second)
                // - throughput = [total number of completed processes] / [overall completed time]
                // - overall, "total" will be "completed time" since this variable tracks completion time
                // - since the "turnaround time" is in "milli-second", convert it to "second" by divide 1,000
                double throughput = np / ((double) total / 1000);

                // display results:
                // - average waiting time (milli-seconds)
                // - average turnaround time (milli-seconds)
                // - CPU Utilization (%)
                // - Throughput (processes / second)
                MessageBox.Show("** Round Robin (RR) with " + np + " processes **\n\n"
                                + "Average Waiting Time (AWT) = " + averageWaitTime.ToString("0.0##") + " ms\n\n"
                                + "Average Turnaround Time (ATT) = " + averageTurnaroundTime.ToString("0.0##") + " ms\n\n"
                                + "CPU Utilization = " + ((double)cpu.NextValue()).ToString("0.0##") + " %\n\n"
                                + "Throughput = " + throughput.ToString("0.0##") + " processes/second\n"
                                , "Round Robin (RR) Result", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
        }
    }
}

