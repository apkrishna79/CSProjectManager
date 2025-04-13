/*
 * Prologue
Created By: Jackson Wunderlich
Date Created: 4/13/25
Last Revised By: Jackson Wunderlich
Date Revised: 4/13/25
Purpose: allows users to take notes for a meeting and save them to the database
Preconditions: MongoDBService, CalendarService, MeetingMinutes Service, instances properly initialized and injected; MeetingMinutes model must be correctly defined
Postconditions: Users can update meeting minutes for a given meeting
Error and exceptions: ArgumentNullException (required field empty), FormatException (invalid data input)
Side effects: N/A
Invariants: _calendarService and _meetingMinutesService fields are always initialized with a valid instance
Other faults: N/A
*/

using CS_Project_Manager.Models;
using CS_Project_Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;

namespace CS_Project_Manager.Pages
{
    public class MeetingMinutesModel : PageModel
    {
        private readonly CalendarService _calendarService;
        private readonly MeetingMinutesService _meetingMinutesService;

        // bound properties for necessary variables
        [BindProperty(SupportsGet = true)]
        public ObjectId MeetingId { get; set; }

        [BindProperty]
        public MeetingMinutes AssocMinutes { get; set; } = new MeetingMinutes { Notes = "" };

        [BindProperty]
        public string MeetingNotes { get; set; }

        [BindProperty]
        public string MeetingName { get; set; }

        public MeetingMinutesModel(CalendarService calendarService, MeetingMinutesService meetingMinutesService)
        {
            _calendarService = calendarService;
            _meetingMinutesService = meetingMinutesService;
        }

        public async Task OnGetAsync(ObjectId meetingId)
        {
            if (meetingId == ObjectId.Parse(null))
            {
                throw new ArgumentException("Invalid project ID");
            }
            // gets the current meeting and its corresponding minutes
            var foundMinutes = await _meetingMinutesService.GetMinutesByMeetingIdAsync(meetingId);
            var meeting = await _calendarService.GetCalendarItemByIdAsync(meetingId);
            if (foundMinutes == null)
            {
                // if no minutes are found, create a new minutes object and add it to the database
                var newMinutes = new MeetingMinutes { Notes = "", AssocMeetingId = meetingId };
                await _meetingMinutesService.AddMinutesAsync(newMinutes);
                foundMinutes = await _meetingMinutesService.GetMinutesByMeetingIdAsync(meetingId);
            }
            // sets bound properties to minutes values
            AssocMinutes = foundMinutes;
            MeetingNotes = AssocMinutes.Notes;
            MeetingName = meeting.EventName;
            MeetingId = meetingId;

        }

        // updates a minutes item
        public async Task<IActionResult> OnPostUpdateAsync(ObjectId id, ObjectId meetingId)
        {
            var existingMinutes = await _meetingMinutesService.GetMinutesByIdAsync(id);
            if (existingMinutes == null)
            {
                return NotFound();
            }
            // sets the notes to the updated notes changed by the user
            existingMinutes.Notes = MeetingNotes;
            
            await _meetingMinutesService.UpdateMinutesAsync(existingMinutes);

            return RedirectToPage(new { meetingId });
        }

        // exports minutes to a text document
        /*
        public async Task<IActionResult> OnPostExportAsync()
        {
            if (string.IsNullOrWhiteSpace(MeetingNotes))
            {
                MeetingNotes = "No minutes found!";
            }

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                // writes MeetingNotes content to a new text document
                await writer.WriteAsync(MeetingNotes);
                await writer.FlushAsync();
                stream.Position = 0;

                string fileName = "MeetingMinutes" + MeetingName + ".txt";

                return File(stream.ToArray(), "text/plain", fileName);
            }
        }
        */
    }
}
