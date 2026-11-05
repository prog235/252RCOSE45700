import re
from typing import List, Tuple

def parse_truth_blocks(text: str) -> List[Tuple[int, int, str]]:
    """
    반환: [(start, end, content), ...]
    """
    pattern = re.compile(r"#TRUTH\s+(\d+)-(\d+)")
    matches = list(pattern.finditer(text))

    blocks = []

    for i, match in enumerate(matches):
        start = int(match.group(1))
        end = int(match.group(2))

        content_start = match.end()
        content_end = matches[i + 1].start() if i + 1 < len(matches) else len(text)

        content = text[content_start:content_end].strip()
        blocks.append((start, end, content))

    return blocks

def select_truth_block(blocks, truth_level: int) -> str:
    print(f"truthLv = {truth_level}")
    for start, end, content in blocks:
        if start <= truth_level <= end:
            return content
    return ""

def load_truth_for_level(truth_text: str, truth_level: int) -> str:
    blocks = parse_truth_blocks(truth_text)
    return select_truth_block(blocks, truth_level)
