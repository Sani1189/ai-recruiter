const getGrade = (score: number) => {
  // Scores are 1–10; adjust grading thresholds accordingly
  if (score >= 8) return "A";
  if (score >= 7) return "B";
  if (score >= 4) return "C";
  return "D";
};

const getScoreColor = (score: number) => {
  const grade = getGrade(score);
  if (grade === "A") return "bg-green-500";
  if (grade === "B") return "bg-blue-500";
  if (grade === "C") return "bg-yellow-500";
  return "bg-red-500";
};

export { getGrade, getScoreColor };
