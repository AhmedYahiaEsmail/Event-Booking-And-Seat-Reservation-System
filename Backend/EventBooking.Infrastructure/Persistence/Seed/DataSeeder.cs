using EventBooking.Application.Interfaces.Auth;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EventBooking.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    // Default password for every seeded regular user (meets RegisterRequestValidator rules)
    private const string DefaultPassword = "Passw0rd!";
    private const string AdminPassword = "Admin@123";

    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // Idempotency guard: never re-seed a database that already has data.
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;

        // ---------- Local helpers ----------

        User NewUser(string firstName, string lastName, string email, UserRole role, string password)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = passwordHasher.HashPassword(password),
                Role = role
            };
            context.Users.Add(user);
            return user;
        }

        Event NewEvent(
            string title, string description, string location,
            string speakerName, string speakerBio,
            int totalSeats, int startOffsetDays, int durationHours)
        {
            var start = now.AddDays(startOffsetDays);
            var end = start.AddHours(durationHours);

            var ev = new Event
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                StartDateTime = start,
                EndDateTime = end,
                Location = location,
                SpeakerName = speakerName,
                SpeakerBio = speakerBio
            };
            ev.InitializeSeats(totalSeats);
            context.Events.Add(ev);
            return ev;
        }

        Reservation Reserve(Event ev, User user, int seats, DateTimeOffset bookingTime)
        {
            ev.ReserveSeats(seats, bookingTime);
            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EventId = ev.Id,
                NumberOfSeats = seats,
                BookingDateTime = bookingTime
            };
            context.Reservations.Add(reservation);
            return reservation;
        }

        void CancelReservation(Reservation reservation, Event ev)
        {
            reservation.Cancel();
            ev.ReleaseSeats(reservation.NumberOfSeats);
        }

        // ---------- Users ----------

        var admin = NewUser("Admin", "User", "admin@eventbooking.com", UserRole.Admin, AdminPassword);

        var u0 = NewUser("Ahmed", "Hassan", "ahmed.hassan@eventbooking.com", UserRole.User, DefaultPassword);
        var u1 = NewUser("Mona", "Ibrahim", "mona.ibrahim@eventbooking.com", UserRole.User, DefaultPassword);
        var u2 = NewUser("Youssef", "Adel", "youssef.adel@eventbooking.com", UserRole.User, DefaultPassword);
        var u3 = NewUser("Sara", "Mahmoud", "sara.mahmoud@eventbooking.com", UserRole.User, DefaultPassword);
        var u4 = NewUser("Omar", "Khaled", "omar.khaled@eventbooking.com", UserRole.User, DefaultPassword);
        var u5 = NewUser("Nourhan", "Tarek", "nourhan.tarek@eventbooking.com", UserRole.User, DefaultPassword);
        var u6 = NewUser("Khaled", "Fathy", "khaled.fathy@eventbooking.com", UserRole.User, DefaultPassword);
        var u7 = NewUser("Yasmin", "Samir", "yasmin.samir@eventbooking.com", UserRole.User, DefaultPassword);
        var u8 = NewUser("Mahmoud", "Reda", "mahmoud.reda@eventbooking.com", UserRole.User, DefaultPassword);
        var u9 = NewUser("Dina", "Farouk", "dina.farouk@eventbooking.com", UserRole.User, DefaultPassword);
        var u10 = NewUser("Amr", "Salah", "amr.salah@eventbooking.com", UserRole.User, DefaultPassword);
        var u11 = NewUser("Heba", "Nabil", "heba.nabil@eventbooking.com", UserRole.User, DefaultPassword);


        // ================= DRAFT EVENTS (10 Events - far future, never published, no reservations) =================

        NewEvent("Advanced Kubernetes Orchestration Workshop", "Hands-on deep dive into K8s scheduling, autoscaling.", "Innovation Lab", "Tarek Younis", "Senior Platform Engineer.", 40, 90, 6);
        NewEvent("Introduction to Rust for Systems Programming", "Ownership, borrowing, and safe systems software.", "Training Room B", "Laila Mansour", "Systems engineer and Rust contributor.", 35, 75, 4);
        NewEvent("AI Ethics & Responsible Machine Learning", "Fairness, bias, and governance frameworks.", "Main Auditorium", "Dr. Karim Aziz", "AI researcher focused on responsible ML.", 50, 100, 3);
        NewEvent("Advanced Go Concurrency Patterns", "Mastering Goroutines, Channels, and Select.", "Room 404", "Sami Nabil", "Go Backend Developer.", 30, 80, 5);
        NewEvent("Intro to Quantum Computing", "Understanding Qubits, Superposition, and Entanglement.", "Science Lab", "Dr. Hoda Kamel", "Quantum Physics Professor.", 60, 120, 3);
        NewEvent("Building Web3 dApps", "Smart contracts using Solidity and Hardhat.", "Innovation Lab", "Ramy Saeed", "Web3 Engineer.", 45, 110, 4);
        NewEvent("Serverless Architecture with AWS", "AWS Lambda, API Gateway, and DynamoDB.", "Training Room A", "Noha Adel", "AWS Certified Solutions Architect.", 50, 95, 6);
        NewEvent("SvelteKit Masterclass", "Building blazing fast web apps with Svelte.", "Room 301", "Kareem Taha", "Frontend Specialist.", 35, 85, 4);
        NewEvent("iOS App Dev with Swift", "SwiftUI and Combine for modern iOS apps.", "Mac Lab", "Rasha Fawzy", "Senior iOS Developer.", 40, 105, 5);
        NewEvent("Game Dev with Unreal Engine 5", "Nanite, Lumen, and Blueprint visual scripting.", "Gaming Arena", "Bassel Amr", "Game Developer.", 25, 130, 6);


        // ================= PUBLISHED / UPCOMING EVENTS (18 Events) =================

        var cloudSummit = NewEvent("Cloud Native Architecture Summit 2026", "Full-day summit covering multi-cloud strategy.", "Main Auditorium", "Multiple Speakers", "Industry cloud architects.", 200, 45, 8);
        cloudSummit.Publish(now);
        Reserve(cloudSummit, u0, 5, now); Reserve(cloudSummit, u2, 8, now); Reserve(cloudSummit, u4, 6, now); Reserve(cloudSummit, u6, 10, now); Reserve(cloudSummit, u8, 7, now); Reserve(cloudSummit, u10, 4, now);

        var reactPatterns = NewEvent("Modern React Patterns & Server Components", "Server components and SSR.", "Training Room A", "Nadia Selim", "Frontend architect.", 60, 20, 4);
        reactPatterns.Publish(now);
        Reserve(reactPatterns, u1, 6, now); Reserve(reactPatterns, u3, 8, now); Reserve(reactPatterns, u5, 7, now); Reserve(reactPatterns, u7, 9, now); Reserve(reactPatterns, u9, 6, now); Reserve(reactPatterns, u11, 5, now);

        var cyberSecFund = NewEvent("Cybersecurity Fundamentals for Developers", "OWASP Top 10, threat modeling.", "Training Room B", "Hossam Zaki", "Application security lead.", 40, 15, 5);
        cyberSecFund.Publish(now);
        Reserve(cyberSecFund, u0, 3, now); Reserve(cyberSecFund, u2, 4, now); Reserve(cyberSecFund, u4, 5, now); Reserve(cyberSecFund, u6, 3, now); Reserve(cyberSecFund, u8, 2, now);

        var microservices = NewEvent("Building Scalable Microservices with .NET 10", "Resilient microservices using .NET 10.", "Innovation Lab", "Mostafa Kamal", "Backend architect.", 80, 25, 6);
        microservices.Publish(now);
        Reserve(microservices, u1, 5, now); Reserve(microservices, u3, 6, now); Reserve(microservices, u5, 4, now); Reserve(microservices, u7, 8, now); Reserve(microservices, u9, 5, now); Reserve(microservices, u11, 2, now);

        var devOpsCiCd = NewEvent("DevOps CI/CD Pipeline Masterclass", "End-to-end automated pipelines.", "Training Room A", "Rania Adly", "DevOps lead.", 50, 10, 5);
        devOpsCiCd.Publish(now);
        Reserve(devOpsCiCd, u0, 8, now); Reserve(devOpsCiCd, u2, 10, now); Reserve(devOpsCiCd, u4, 9, now); Reserve(devOpsCiCd, u6, 7, now); Reserve(devOpsCiCd, u8, 8, now); Reserve(devOpsCiCd, u10, 6, now); // 48/50 booked

        var dataEngineering = NewEvent("Data Engineering with Apache Spark", "Batch and streaming pipelines.", "Training Room B", "Fady Nassif", "Data engineer.", 45, 12, 5);
        dataEngineering.Publish(now);
        Reserve(dataEngineering, u1, 9, now); Reserve(dataEngineering, u3, 8, now); Reserve(dataEngineering, u5, 7, now); Reserve(dataEngineering, u7, 10, now); Reserve(dataEngineering, u9, 6, now); Reserve(dataEngineering, u11, 4, now); // 44/45 booked

        var uxWorkshop = NewEvent("UX Design Thinking Workshop", "Prototyping and usability testing.", "Innovation Lab", "Salma Younis", "Senior UX designer.", 30, 8, 4);
        uxWorkshop.Publish(now);
        Reserve(uxWorkshop, u0, 5, now); Reserve(uxWorkshop, u2, 6, now); Reserve(uxWorkshop, u4, 4, now); Reserve(uxWorkshop, u6, 5, now); Reserve(uxWorkshop, u8, 6, now); Reserve(uxWorkshop, u10, 4, now); // 30/30 Sold out

        var mlModels = NewEvent("Machine Learning Models in Production", "Deploying and monitoring ML models via MLOps.", "Main Auditorium", "Dr. Yasser Amin", "Lead Data Scientist.", 100, 35, 5);
        mlModels.Publish(now);
        Reserve(mlModels, u0, 10, now); Reserve(mlModels, u5, 15, now); Reserve(mlModels, u11, 5, now);

        var tailwind = NewEvent("Advanced CSS and Tailwind CSS", "Utility-first design systems and custom configurations.", "Room 202", "Dina Adel", "UI Developer.", 40, 14, 4);
        tailwind.Publish(now);
        Reserve(tailwind, u1, 5, now); Reserve(tailwind, u4, 2, now);

        var nextjs = NewEvent("Fullstack Next.js 14", "App router, server actions, and edge rendering.", "Innovation Lab", "Amjad Fathy", "Fullstack Developer.", 50, 18, 6);
        nextjs.Publish(now);
        Reserve(nextjs, u2, 4, now); Reserve(nextjs, u6, 8, now); Reserve(nextjs, u9, 2, now);

        var nodejsPerf = NewEvent("Node.js Performance Tuning", "Memory leaks, V8 engine internals, and profiling.", "Training Room B", "Kamel Reda", "Backend Engineer.", 35, 22, 4);
        nodejsPerf.Publish(now);
        Reserve(nodejsPerf, u3, 5, now); Reserve(nodejsPerf, u7, 6, now);

        var postgres = NewEvent("Mastering PostgreSQL", "Advanced indexing, partitioning, and query optimization.", "Room 101", "Mona Helmy", "Database Administrator.", 45, 28, 5);
        postgres.Publish(now);
        Reserve(postgres, u0, 7, now); Reserve(postgres, u8, 3, now); Reserve(postgres, u10, 5, now);

        var redisCache = NewEvent("Redis for Caching & Microservices", "Pub/Sub, caching strategies, and Redis Streams.", "Training Room A", "Hassan Tarek", "Software Architect.", 40, 30, 3);
        redisCache.Publish(now);
        Reserve(redisCache, u4, 10, now); Reserve(redisCache, u1, 4, now);

        var kafkaArch = NewEvent("Kafka Event-Driven Architecture", "Topics, consumers, and KSQL streams.", "Main Auditorium", "Tamer Zaki", "Data Architect.", 80, 40, 6);
        kafkaArch.Publish(now);
        Reserve(kafkaArch, u5, 12, now); Reserve(kafkaArch, u6, 8, now);

        var figmaDevs = NewEvent("Figma for Developers", "Bridging the gap between design and code.", "Innovation Lab", "Yara Samir", "UX/UI Lead.", 30, 7, 3);
        figmaDevs.Publish(now);
        Reserve(figmaDevs, u2, 5, now); Reserve(figmaDevs, u9, 3, now);

        var prodMgmt = NewEvent("Product Management 101", "Agile methodologies, user stories, and roadmaps.", "Room 205", "Karim Shaker", "Product Manager.", 50, 11, 5);
        prodMgmt.Publish(now);
        Reserve(prodMgmt, u3, 8, now); Reserve(prodMgmt, u11, 6, now);

        var threatHunting = NewEvent("Cyber Threat Hunting", "Proactive security and identifying advanced persistent threats.", "Security Lab", "Hisham Farouk", "Cybersecurity Analyst.", 25, 26, 4);
        threatHunting.Publish(now);
        Reserve(threatHunting, u7, 4, now); Reserve(threatHunting, u8, 5, now);

        var azureFund = NewEvent("Azure Fundamentals AZ-900", "Cloud concepts, Azure services, and pricing.", "Training Room B", "Salma Nabil", "Microsoft Certified Trainer.", 60, 32, 6);
        azureFund.Publish(now);
        Reserve(azureFund, u0, 10, now); Reserve(azureFund, u1, 5, now); Reserve(azureFund, u10, 8, now);


        // ================= COMPLETED EVENTS (15 Events - past, with real booking history) =================

        var bootcamp = NewEvent("Full-Stack Web Development Bootcamp", "React, .NET APIs, deployment.", "Main Auditorium", "Tamer Anwar", "Technical trainer.", 100, -30, 8);
        bootcamp.Publish(now.AddDays(-40));
        Reserve(bootcamp, u0, 10, now.AddDays(-38));
        var bootcampCancelledRes = Reserve(bootcamp, u2, 12, now.AddDays(-37));
        Reserve(bootcamp, u4, 8, now.AddDays(-36)); Reserve(bootcamp, u6, 15, now.AddDays(-35)); Reserve(bootcamp, u8, 10, now.AddDays(-34)); Reserve(bootcamp, u10, 9, now.AddDays(-33));
        CancelReservation(bootcampCancelledRes, bootcamp);
        bootcamp.Complete(now);

        var dockerWorkshop = NewEvent("Introduction to Docker & Containerization", "Image optimization, orchestration.", "Training Room A", "Peter Nabil", "DevOps engineer.", 50, -15, 4);
        dockerWorkshop.Publish(now.AddDays(-25));
        Reserve(dockerWorkshop, u1, 8, now.AddDays(-23)); Reserve(dockerWorkshop, u3, 7, now.AddDays(-22));
        var dockerCancelledRes = Reserve(dockerWorkshop, u5, 6, now.AddDays(-21));
        Reserve(dockerWorkshop, u7, 9, now.AddDays(-20)); Reserve(dockerWorkshop, u9, 5, now.AddDays(-19));
        CancelReservation(dockerCancelledRes, dockerWorkshop);
        dockerWorkshop.Complete(now);

        var agileScrum = NewEvent("Agile & Scrum Certification Prep", "Scrum simulations.", "Training Room B", "Mariam Younan", "Agile Coach.", 35, -45, 6);
        agileScrum.Publish(now.AddDays(-55));
        Reserve(agileScrum, u0, 6, now.AddDays(-53)); Reserve(agileScrum, u2, 5, now.AddDays(-52)); Reserve(agileScrum, u4, 7, now.AddDays(-51)); Reserve(agileScrum, u6, 4, now.AddDays(-50)); Reserve(agileScrum, u8, 6, now.AddDays(-49));
        agileScrum.Complete(now);

        var graphQlWorkshop = NewEvent("GraphQL API Design Best Practices", "Schema design, N+1 pitfalls.", "Innovation Lab", "Sherif Gouda", "API architect.", 40, -60, 3);
        graphQlWorkshop.Publish(now.AddDays(-70));
        Reserve(graphQlWorkshop, u1, 7, now.AddDays(-68)); Reserve(graphQlWorkshop, u3, 6, now.AddDays(-67)); Reserve(graphQlWorkshop, u5, 8, now.AddDays(-66));
        var graphQlCancelledRes = Reserve(graphQlWorkshop, u7, 5, now.AddDays(-65));
        Reserve(graphQlWorkshop, u9, 4, now.AddDays(-64));
        CancelReservation(graphQlCancelledRes, graphQlWorkshop);
        graphQlWorkshop.Complete(now);

        var pythonDataScience = NewEvent("Python for Data Science", "Pandas, NumPy, and visualization.", "Training Room A", "Aya Ramzy", "Data scientist.", 55, -20, 5);
        pythonDataScience.Publish(now.AddDays(-30));
        Reserve(pythonDataScience, u0, 8, now.AddDays(-28)); Reserve(pythonDataScience, u2, 9, now.AddDays(-27)); Reserve(pythonDataScience, u4, 7, now.AddDays(-26)); Reserve(pythonDataScience, u6, 6, now.AddDays(-25)); Reserve(pythonDataScience, u8, 5, now.AddDays(-24)); Reserve(pythonDataScience, u10, 5, now.AddDays(-23));
        pythonDataScience.Complete(now);

        var vue3Api = NewEvent("Vue 3 Composition API", "Reactivity and modern Vue architecture.", "Room 302", "Omar Taha", "Frontend Dev.", 40, -25, 4);
        vue3Api.Publish(now.AddDays(-35));
        Reserve(vue3Api, u1, 10, now.AddDays(-30)); Reserve(vue3Api, u3, 5, now.AddDays(-28));
        vue3Api.Complete(now);

        var angular17 = NewEvent("Angular 17 Deep Dive", "Standalone components and Signals.", "Innovation Lab", "Tarek Helmy", "Google Developer Expert.", 50, -40, 6);
        angular17.Publish(now.AddDays(-50));
        Reserve(angular17, u5, 12, now.AddDays(-48)); Reserve(angular17, u7, 6, now.AddDays(-45));
        angular17.Complete(now);

        var dotnet8 = NewEvent("C# 12 and .NET 8 New Features", "Primary constructors, AOT compilation.", "Main Auditorium", "Maged Ali", "Backend Lead.", 120, -10, 4);
        dotnet8.Publish(now.AddDays(-20));
        Reserve(dotnet8, u0, 15, now.AddDays(-18)); Reserve(dotnet8, u2, 20, now.AddDays(-15)); Reserve(dotnet8, u4, 10, now.AddDays(-12));
        dotnet8.Complete(now);

        var java21 = NewEvent("Java 21 Virtual Threads", "Project Loom and scalable concurrency.", "Training Room B", "Ramy Hassan", "Java Architect.", 45, -55, 5);
        java21.Publish(now.AddDays(-65));
        Reserve(java21, u6, 8, now.AddDays(-60)); Reserve(java21, u8, 7, now.AddDays(-58));
        java21.Complete(now);

        var pythonScraping = NewEvent("Python Web Scraping", "BeautifulSoup, Scrapy, and Selenium.", "Room 102", "Dalia Samir", "Data Engineer.", 35, -70, 4);
        pythonScraping.Publish(now.AddDays(-80));
        Reserve(pythonScraping, u9, 5, now.AddDays(-75)); Reserve(pythonScraping, u11, 4, now.AddDays(-72));
        pythonScraping.Complete(now);

        var dataVisD3 = NewEvent("Data Vis with D3.js", "Interactive charts and SVG manipulation.", "Innovation Lab", "Kamal Riad", "Frontend Engineer.", 30, -12, 5);
        dataVisD3.Publish(now.AddDays(-22));
        Reserve(dataVisD3, u1, 6, now.AddDays(-20)); Reserve(dataVisD3, u10, 4, now.AddDays(-18));
        dataVisD3.Complete(now);

        var powerBi = NewEvent("PowerBI Dashboards Masterclass", "DAX, data modeling, and business insights.", "Training Room A", "Laila Amr", "BI Developer.", 60, -32, 6);
        powerBi.Publish(now.AddDays(-42));
        Reserve(powerBi, u3, 10, now.AddDays(-40)); Reserve(powerBi, u5, 8, now.AddDays(-38));
        powerBi.Complete(now);

        var snowflake = NewEvent("Snowflake Cloud Data Warehouse", "Architecture, Snowpark, and optimization.", "Room 401", "Amir Fouad", "Data Architect.", 50, -85, 4);
        snowflake.Publish(now.AddDays(-95));
        Reserve(snowflake, u7, 12, now.AddDays(-90)); Reserve(snowflake, u0, 5, now.AddDays(-88));
        snowflake.Complete(now);

        var terraform = NewEvent("Terraform for AWS Infrastructure", "Infrastructure as Code, state management.", "Main Auditorium", "Hany Saad", "Cloud Engineer.", 80, -90, 5);
        terraform.Publish(now.AddDays(-100));
        Reserve(terraform, u2, 15, now.AddDays(-98)); Reserve(terraform, u4, 10, now.AddDays(-95));
        terraform.Complete(now);

        var linuxSys = NewEvent("Linux SysAdmin Basics", "Bash scripting, cron jobs, and permissions.", "Training Room B", "Khaled Youssef", "System Administrator.", 40, -100, 4);
        linuxSys.Publish(now.AddDays(-110));
        Reserve(linuxSys, u6, 7, now.AddDays(-108)); Reserve(linuxSys, u8, 5, now.AddDays(-105));
        linuxSys.Complete(now);


        // ================= CANCELLED EVENTS (7 Events) =================

        var blockchainFund = NewEvent("Blockchain & Web3 Fundamentals", "Smart contracts, consensus mechanisms.", "Training Room B", "Ziad Hamdy", "Blockchain developer.", 60, 18, 4);
        blockchainFund.Publish(now);
        Reserve(blockchainFund, u1, 3, now); Reserve(blockchainFund, u3, 2, now);
        blockchainFund.Cancel(); // Cancelled after seats booked

        NewEvent("Legacy System Migration Strategies", "Migrating monolithic systems.", "Training Room A", "Wael Fahmy", "Enterprise architect.", 25, 50, 3)
            .Cancel(); // Cancelled from Draft

        var leadershipConf = NewEvent("Annual Tech Leadership Conference", "Scaling teams and culture.", "Main Auditorium", "Multiple Speakers", "Engineering directors.", 150, 35, 8);
        leadershipConf.Publish(now);
        leadershipConf.Cancel(); // Cancelled due to venue, no bookings yet

        var wasmRust = NewEvent("WebAssembly with Rust", "Running Rust in the browser safely.", "Innovation Lab", "Salma Adel", "WebAssembly Contributor.", 40, 25, 4);
        wasmRust.Publish(now);
        Reserve(wasmRust, u5, 4, now); Reserve(wasmRust, u7, 3, now);
        wasmRust.Cancel();

        NewEvent("Metaverse 101: VR and Beyond", "Exploring virtual spaces and VR hardware.", "Room 201", "Tarek Zaki", "VR Specialist.", 50, 60, 3)
            .Cancel();

        var nftsCreators = NewEvent("NFTs for Creators & Artists", "Minting, wallets, and marketplaces.", "Training Room A", "Dina Fawzy", "Digital Artist.", 30, 15, 2);
        nftsCreators.Publish(now);
        Reserve(nftsCreators, u9, 2, now);
        nftsCreators.Cancel();

        var rubyRails = NewEvent("Ruby on Rails 7", "Hotwire, Turbo, and modern Rails apps.", "Room 303", "Ahmed Farag", "Rails Developer.", 45, 40, 5);
        rubyRails.Publish(now);
        rubyRails.Cancel();


        await context.SaveChangesAsync();
    }
}