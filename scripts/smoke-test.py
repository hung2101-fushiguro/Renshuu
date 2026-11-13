"""Exercise the real Razor forms against an isolated development database.

Usage: python scripts/smoke-test.py http://localhost:5186 --allow-test-writes
Creates two small attempts and deletes only those attempts at the end.
Never use against a shared/production database: attempts affect the latest-answer view.
"""
import html
import http.cookiejar
import re
import sys
import urllib.parse
import urllib.request

if len(sys.argv) != 3 or sys.argv[2] != "--allow-test-writes":
    raise SystemExit(__doc__)
BASE = sys.argv[1].rstrip("/")
if urllib.parse.urlparse(BASE).hostname not in ("localhost", "127.0.0.1"):
    raise SystemExit("This development smoke test is restricted to loopback.")
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
created = []
passed = []

def fetch(path, values=None):
    data = urllib.parse.urlencode(values).encode() if values is not None else None
    with client.open(BASE + path, data=data, timeout=20) as response:
        text = response.read().decode("utf-8")
        assert response.status == 200, (path, response.status)
        assert "<h1>Đã xảy ra lỗi" not in text, path
        return text, urllib.parse.urlparse(response.url).path

def token(text):
    match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', text)
    assert match, "Antiforgery token missing"
    return html.unescape(match[1])

def post(path, source, values):
    return fetch(path, {**values, "__RequestVerificationToken": token(source)})

try:
    home, _ = fetch("/")
    assert "Không gian luyện tập" in home and "launch-experience" in home
    option = re.search(r'<option value="(\d+)"[^>]*data-count="([1-9]\d*)"', home)
    assert option, "Need a seeded subject with questions"
    subject_id = option[1]
    passed.append("Dashboard renders database-backed subject options")
    for route in ("/Wrong", "/History", "/Admin", "/Admin/Questions", "/Admin/QuestionEdit", "/Admin/Import", "/Admin/Paste", "/Admin/AddMany", "/Admin/Readings"):
        text, _ = fetch(route)
        assert 'class="app-shell"' in text and "<h1>" in text, route
    passed.append("Nine study/admin pages render with the shared layout")
    for asset in ("/css/workspace.css", "/js/workspace.js", "/js/dashboard.js", "/images/icons.svg", "/favicon.svg", "/assets/motion/motion.js", "/assets/motion/motion.css"):
        fetch(asset)
    passed.append("All seven local UI assets return HTTP 200")
    invalid, path = post("/", home, {"Input.SubjectId": subject_id, "Input.QuestionCount": "201"})
    assert path == "/" and ("validation-summary-errors" in invalid or "field-validation-error" in invalid)
    passed.append("Invalid question counts retain server validation")
    take, path = post("/", home, {"Input.SubjectId": subject_id, "Input.QuestionCount": "2", "Input.TimeLimitMinutes": "10"})
    assert path.startswith("/Test/Take/"), path
    attempt_id = int(path.rsplit("/", 1)[1]); created.append(attempt_id)
    ids = list(dict.fromkeys(re.findall(r'name="Answers\[(\d+)\]"', take)))
    assert len(ids) >= 2 and 'id="timer"' in take and 'class="question-jump"' in take
    assert "CorrectOption" not in take
    passed.append("Start creates an attempt, timer and question navigation")
    result, path = post(path, take, {"id": attempt_id})
    assert path == "/Test/Result/" + str(attempt_id) and "score-ring" in result
    assert 'class="question is-wrong"' in result
    passed.append("Blank answers are graded wrong and result summary renders")
    wrong, _ = fetch("/Wrong?subjectId=" + subject_id)
    assert "Làm lại các câu sai" in wrong
    retry, retry_path = post("/Wrong", wrong, {"SubjectId": subject_id})
    retry_id = int(retry_path.rsplit("/", 1)[1]); created.append(retry_id)
    retry_ids = list(dict.fromkeys(re.findall(r'name="Answers\[(\d+)\]"', retry)))
    # The result page is the authorized source of correct answers for review.
    # Match question content across randomized attempts to derive the retry answers.
    answers_by_content = {}
    for block in re.split(r'<section class="question ', result)[1:]:
        content = re.search(r'<p class="question-content">(.*?)</p>', block, re.S)[1]
        key = re.search(r'Đáp án: ([A-D])', html.unescape(block))[1]
        answers_by_content[content] = key
    answers = {"id": retry_id}
    for block in re.split(r'<section class="question"', retry)[1:]:
        question_id = re.search(r'name="Answers\[(\d+)\]"', block)[1]
        content = re.search(r'<p class="question-content">(.*?)</p>', block, re.S)[1]
        assert content in answers_by_content, "Run only with a clean isolated database"
        answers["Answers[" + question_id + "]"] = answers_by_content[content]
    retry_result, _ = post(retry_path, retry, answers)
    assert 'class="question is-wrong"' not in retry_result
    assert len(retry_ids) == len(ids)
    passed.append("Wrong-answer retry uses the same questions and grades all correct")
    wrong_after, _ = fetch("/Wrong?subjectId=" + subject_id)
    assert "Làm lại các câu sai" not in wrong_after
    passed.append("Correct retry removes questions from the latest-wrong list")
    history, _ = fetch("/History")
    assert all('/Test/Result/' + str(value) in history for value in created)
    home_after, _ = fetch("/")
    assert all('/Test/Result/' + str(value) in home_after for value in created)
    passed.append("History and dashboard show both completed attempts")
    again, _ = post("/Test/Take/" + str(attempt_id), take, {"id": attempt_id})
    assert 'class="question is-wrong"' in again
    passed.append("Resubmission preserves the original grade")
finally:
    for attempt_id in reversed(created):
        history, _ = fetch("/History")
        post("/History?handler=Delete", history, {"id": attempt_id})
    print("\n".join("PASS " + check for check in passed))
    print("Cleaned up " + str(len(created)) + " test attempts.")
