use std::{fmt::Debug, rc::Rc};

/// Evaluate if program are of the same shape rather then being exactly the same.
pub trait ProgramEquivalent {
  fn program_equivalent(&self, other: &Self) -> bool;
}
pub fn assert_equivalent<T: ProgramEquivalent + Debug>(expected: &T, actual: &T) {
  assert!(
    expected.program_equivalent(actual),
    "Elements are not equivalent \nexpected: {expected:#?}\nactual: {actual:#?}"
  )
}

impl<T: ProgramEquivalent> ProgramEquivalent for Box<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.as_ref().program_equivalent(b.as_ref())
  }
}

impl<T: ProgramEquivalent> ProgramEquivalent for Option<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    match (self, b) {
      (Some(a), Some(b)) => a.program_equivalent(b),
      (None, None) => true,
      _ => false,
    }
  }
}

impl<T: ProgramEquivalent> ProgramEquivalent for Vec<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    self
      .iter()
      .zip(b.iter())
      .all(|(a, b)| a.program_equivalent(b))
  }
}

impl<T: ProgramEquivalent> ProgramEquivalent for Rc<T> {
  fn program_equivalent(&self, b: &Self) -> bool {
    self.as_ref().program_equivalent(b.as_ref())
  }
}
